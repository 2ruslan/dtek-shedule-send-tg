using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.TextInfo;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.DTEK
{
    public class SiteAnalyzer(ILogger logger, TextInfoRepository repository) : ISiteAnalyzer
    {
        const string site = "https://www.dtek-krem.com.ua";

        public ISiteAnalyzerResult Analyze()
        {
            logger.LogInformation("Start Analyze");

            using WebClient client = new();

            string source = GetSiteSource(client, $"{site}/ua/shutdowns");

            var pictureUrl = GetPictureUrl(source);
            if (!string.IsNullOrEmpty(pictureUrl))
            {
                var fileName = StorePicFromUrl(client, pictureUrl);
                return new SiteAnalyzerPictureResult() { PIctureFile = fileName };
            }

            var info = GetInfoText(source);
            if (!string.IsNullOrEmpty(info))
            {
                return new SiteAnalyzerTextResult() { Text = info};
            }
            
            return null;
        }

        private string GetSiteSource(WebClient client, string url)
            => client.DownloadString(url);
      
        private string GetPictureUrl(string source)
        {
            logger.LogInformation("Start GetPictureUrl");

            try
            {
                var posStart = source.IndexOf("<img src=\"/media/page/pag") + 10;
                var posEnd = source.IndexOf(".jpg", posStart) + 4;

                if (posEnd < posStart)
                    return string.Empty;

                var url = $"{site}{source.Substring(posStart, posEnd - posStart)}";

                logger.LogInformation("GetPictureUrl : {0}", url);

                return url;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetPictureUrl");
            }

            logger.LogInformation("end GetPictureUrl");

            return string.Empty;
        }

        private string StorePicFromUrl(WebClient client, string url)
        {
            logger.LogInformation("Start StorePicFromUrl");

            var fileName = url.Split('/').LastOrDefault();

            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var dir = Path.Combine(Environment.CurrentDirectory, "pict");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            fileName = Path.Combine(dir, fileName);

            if (File.Exists(fileName))
            {
                logger.LogInformation("File exists {0}", fileName);
                return string.Empty;
            }

            try
            {
                logger.LogInformation("Try Download {0} to file {1}", url, fileName);
                client.DownloadFile(url, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DownloadFile");
                return string.Empty;
            }

            logger.LogInformation("StorePicFromUrl file : {0}", fileName);

            logger.LogInformation("End StorePicFromUrl");

            return fileName;
        }

        private string GetInfoText(string source)
        {
            logger.LogInformation("start GetInfoText");

            var infoTexts = repository.GetTextInfo();
            var lastMessage = repository.GetLastInfoMessage();

            try
            {
                foreach(var textInfo in infoTexts)
                {
                    var m = Regex.Match(source, textInfo.Regex);

                    if (m != null)
                    {
                        if (string.Equals(m.Value, lastMessage, StringComparison.InvariantCultureIgnoreCase))
                            return string.Empty;

                        repository.StoreLastInfoMessage(m.Value);
                        return textInfo.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetInfoText");
            }

            logger.LogInformation("end GetInfoText");
            return string.Empty;
        }
    }
}
