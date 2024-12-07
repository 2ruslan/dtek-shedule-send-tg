using Common;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.Shedule;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace DtekSheduleSendTg.DTEK
{
    public class DtekShedule(ILogger logger) : IDtekShedule
    {
        public string GetSchedule(long group)
            => currentShedule.FirstOrDefault(x => x.Group == group)?.SheduleString;
        
        private  IList<SheduleData> currentShedule = new List<SheduleData>();

        public bool AnalyzeFile(string file)
        {
            try
            {
                currentShedule = GetShedulesFromFile(file);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AnalyzeFile");
                return false;
            }
            return true;
        }

        public string GetFullPictureDescription(long group, string firsttLine, string linePatern, string leadingSymbol)
        {
            var sheduleString = currentShedule.FirstOrDefault(x => x.Group == group)?.SheduleString ?? string.Empty;

            var sb = new StringBuilder();
            
            try
            {
                var h = 0;
                bool isOpenD = false;
                var startPeriod = 0;

                foreach (var s in sheduleString)
                {
                    if (s == '0' && !isOpenD)
                    {
                        startPeriod = h;
                        isOpenD = true;
                    }

                    if (s == '1' && isOpenD)
                    {
                        sb.AppendLine(TextHelper.GetFomatedLine(linePatern, leadingSymbol, startPeriod, h));
                        isOpenD = false;
                    }

                    h++;
                }

                if (isOpenD)
                    sb.AppendLine(TextHelper.GetFomatedLine(linePatern, leadingSymbol, startPeriod, 24));

                if (sb.Length == 0)
                    sb.Append("<b>    - не планується</b>");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetSheduleDescription");
            }

            sb.Insert(0, $"{firsttLine}\r\n");

            return sb.ToString();
        }

        private IList<SheduleData> GetShedulesFromFile(string file)
        {
            logger.LogInformation("Start GetShedules");

            var result = new List<SheduleData>();

            try
            {
                var img = Image.Load<Rgba32>(file);
                var dim = PictureDimensionHelper.GetParamsFormImage(logger, img);
                
                int group = 1;

                foreach (var g in dim.GroupCoord)
                {
                    var sb = new StringBuilder();

                    foreach(var t in dim.TimeCoord)
                    {
                        
                        int average = GetAverage(img, t, g);

                        sb.Append(average > 240 ? "1" : "0");
                    }
                    Console.WriteLine("--");
                    result.Add(new SheduleData() { Group = group++, SheduleString = sb.ToString() });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetShedulesFromFile");
            }

            return result;
        }

        private static int GetAverage(Image<Rgba32> img, int t, int g)
        {
            double average = 0;

            var tm = new int[] {t - 1, t, t + 1};
            var gm = new int[] {g - 1, g, g + 1 };

            foreach (var tp in tm)
                foreach (var gp in gm)
                {
                    var colorPixel = img[tp, gp];
                    average += (((colorPixel.R + colorPixel.B + colorPixel.G) / 3.0) / 9.0);
                }

            return (int)average;
        }
    }
}
