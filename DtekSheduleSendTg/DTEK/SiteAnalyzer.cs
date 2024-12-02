using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace DtekSheduleSendTg.DTEK
{
    public class SiteAnalyzer(  ILogger logger, 
                                ITextInfoRepository repository, 
                                ISiteSource siteSource, 
                                string shedilePicRegex) : ISiteAnalyzer
    {
        public SiteAnalyzerResult Analyze()
        {
            logger.LogInformation("Start Analyze");

            var result = new SiteAnalyzerResult();

            string source = siteSource.GetSource();

            var info = GetInfoText(source);
            if (!string.IsNullOrEmpty(info))
                result.Text = info;

            var pictureUrls = GetPictureUrl(source);
            
            var files = new List<string>();
            foreach (var pictureUrl in pictureUrls)
            {
                var fileName = siteSource.StorePicFromUrl(pictureUrl);
                if (!string.IsNullOrEmpty(fileName))
                    files.Add(fileName);
                    
            }
            result.PIctureFiles = files;

            return result;
        }

        private IEnumerable<string> GetPictureUrl(string source)
        {
            var result = new List<string>();
            logger.LogInformation("Start GetPictureUrl");

            var m = Regex.Matches(source, shedilePicRegex);

            logger.LogInformation("GetPictureUrl count: {0}", m.Count);


            foreach (var m2 in m)
            {
                logger.LogInformation(m2.ToString());
                result.Add(m2.ToString());
            }

            logger.LogInformation("end GetPictureUrl");

            return result.Distinct();
        }

        private string GetInfoText(string source)
        {
            logger.LogInformation("start GetInfoText");

            var infoTexts = repository.GetTextInfo();
            var lastMessage = repository.GetLastInfoMessage();
            logger.LogInformation("GetInfoText lastMessage : [{0}]", lastMessage);

            try
            {
                foreach(var textInfo in infoTexts)
                {
                    var m = Regex.Match(source, textInfo.Regex);

                    if (m != null)
                    {
                        logger.LogInformation("GetInfoText currentMessage : [{0}]", m.Value);

                        if (string.IsNullOrEmpty(m.Value))
                            return string.Empty;

                        if (string.Equals(m.Value, lastMessage, StringComparison.InvariantCultureIgnoreCase))
                            return string.Empty;
                                                
                        repository.StoreLastInfoMessage(m.Value);

                        var message = textInfo.Message != "*"
                            ? textInfo.Message
                            : m.Value
                           ;

                        return message;
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
