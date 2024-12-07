using Common;
using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
            
            foreach (var pictureUrl in pictureUrls)
            {
                var fileName = siteSource.StorePicFromUrl(pictureUrl.Url);
                pictureUrl.FileName = fileName;
            }

            result.PIctureFiles = pictureUrls;

            return result;
        }

        private IEnumerable<PIctureFileInfo> GetPictureUrl(string source)
        {
            var result = new List<PIctureFileInfo>();

            logger.LogInformation("Start GetPictureUrl");

            var m = Regex.Matches(source, shedilePicRegex);

            logger.LogInformation("GetPictureUrl count: {0}", m.Count);

            int daysAdd = 0;
            foreach (var m2 in m)
            {
                var fileName = m2.ToString();
                logger.LogInformation(fileName);

                if (!result.Any(x => string.Equals(x.Url, fileName)))
                { 
                    result.Add(
                        new PIctureFileInfo() {
                            Url = fileName,
                            OnDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysAdd++))
                        }
                    );
                }
            }

            logger.LogInformation("end GetPictureUrl");

            return result;
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

                        if (string.Equals(m.Value.DeleteAllTags().DeleteWhiteSpace(), 
                                          lastMessage.DeleteAllTags().DeleteWhiteSpace(), 
                                          StringComparison.InvariantCultureIgnoreCase))
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
