using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Common;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.DTEK
{
    public class SiteAnalyzer(ILogger logger, ITextInfoRepository repository, ISiteSource siteSource) : ISiteAnalyzer
    {
        public ISiteAnalyzerResult Analyze()
        {
            logger.LogInformation("Start Analyze");

            string source = siteSource.GetSource();

            var pictureUrl = GetPictureUrl(source);
            if (!string.IsNullOrEmpty(pictureUrl))
            {
                var fileName = siteSource.StorePicFromUrl(pictureUrl);
                return new SiteAnalyzerPictureResult() { PIctureFile = fileName };
            }

            var info = GetInfoText(source);
            if (!string.IsNullOrEmpty(info))
            {
                return new SiteAnalyzerTextResult() { Text = info};
            }
            
            return null;
        }

        private string GetPictureUrl(string source)
        {
            logger.LogInformation("Start GetPictureUrl");

            try
            {
                var posStart = source.IndexOf("<img src=\"/media/page/pag") + 10;
                var posEnd = source.IndexOf(".jpg", posStart) + 4;

                if (posEnd < posStart)
                    return string.Empty;

                var url = source.Substring(posStart, posEnd - posStart);

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

                        var message = textInfo.Message != "*"
                            ? textInfo.Message
                            : m.Value
                                //      .Html2Markdown()
                                .DeleteHtmlTags()
                           //     .PrepateAsMarkdown();
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
