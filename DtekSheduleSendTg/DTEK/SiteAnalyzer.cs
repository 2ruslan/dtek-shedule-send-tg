using Common;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.PIctureFileInfo;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.DTEK
{
    public class SiteAnalyzer(  ILogger logger, 
                                IMonitoring monitoring,
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
                var onDateFromFile = PictureHelper.GetDate(fileName, logger, monitoring);

                logger.LogInformation("onDateFromFile={0}", onDateFromFile);

                pictureUrl.FileName = fileName;

                var infoFile = $"{fileName}.info";

                if (onDateFromFile != DateOnly.MinValue)
                    pictureUrl.OnDate = onDateFromFile;
                else
                {
                    var inf = PIctureFileInfoRepository.GetPIctureFileInfo(infoFile);
                    if (inf != null)
                        pictureUrl.OnDate = inf.OnDate;
                }

                PIctureFileInfoRepository.StorePIctureFileInfo(infoFile, pictureUrl);
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
            const string StartDiv = "<div class=\"m-attention__text\">";
            logger.LogInformation("start GetInfoText");

            try
            {
                var startPos = source.IndexOf(StartDiv);
                if (startPos > -1)
                {

                    var endPos =
                        (new List<int>() {
                                source.IndexOf(@"<br", startPos),
                                source.IndexOf(@"</div>", startPos),
                                source.IndexOf("</p>", startPos)
                        })
                        .Where(x => x > startPos)
                        .Min();

                    if (endPos > -1)
                    {
                        var result = source
                            .Substring(startPos, endPos - startPos)
                            //.Replace(StartDiv, string.Empty)
                            .DeleteAllTags()
                            ;

                        if (result.Contains("відключення")    ||
                                result.Contains("стабілізац") ||
                                result.Contains("екстрен")    ||
                                result.Contains("отримувал")  ||
                                result.Contains(DateTime.Now.Day.ToString()) ||
                                result.Contains((DateTime.Now.Day + 1).ToString())
                                )
                            return result;
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
