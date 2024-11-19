using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using System;
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

            var pictureUrl = GetPictureUrl(source);
            if (!string.IsNullOrEmpty(pictureUrl))
            {
                var fileName = siteSource.StorePicFromUrl(pictureUrl);
                if (!string.IsNullOrEmpty(fileName))
                    result.PIctureFile = fileName;
            }

            return result;
        }

        private string GetPictureUrl(string source)
        {
            logger.LogInformation("Start GetPictureUrl");

            var m = Regex.Matches(source, shedilePicRegex); 

            if (m != null)
            {
                logger.LogInformation("GetPictureUrl : {0}", m[^1].Value);
                
                return m[^1].Value;
            }

            logger.LogInformation("end GetPictureUrl");

            return string.Empty;
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
