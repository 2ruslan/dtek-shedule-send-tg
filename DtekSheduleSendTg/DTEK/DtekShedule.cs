using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Common;
using DtekSheduleSendTg.Data.Shedule;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace DtekSheduleSendTg.DTEK
{
    public class DtekShedule(ILogger logger, ISheduleRepository repository) : IDtekShedule
    {
        
        private IEnumerable<int> noSendSheduleGroup;
        private IEnumerable<SheduleData> sheduleGroupDescription;

        public bool AnalyzeFile(string file)
        {
            try
            {
                var currentShedule = GetShedulesFromFile(file);
                var prevShedule = repository.GetShedule();

                noSendSheduleGroup = GetNoSendSheduleGroup(currentShedule, prevShedule);

                sheduleGroupDescription = GetSheduleDescription(currentShedule);

                repository.StoreShedule(currentShedule);
            }
            catch (Exception ex)
            {
                sheduleGroupDescription = new List<SheduleData>();
                repository.StoreShedule(new List<SheduleData>());
                logger.LogError(ex, "AnalyzeFile");
                return false;
            }
            return true;
        }

        public bool IsNoSendPicture2Group(long group)
            => noSendSheduleGroup.Any(x => x == group);

        public string GetFullPictureDescription(long group, string firsttLine)
        {
            var description = sheduleGroupDescription?.FirstOrDefault(x => x.Group == group)?.SheduleString;

            var sb = new StringBuilder(firsttLine);
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine();
                sb.AppendLine("Відключення:");
                sb.Append(description.MarkBold());
            }

            return sb.ToString();
        }

        private List<int> GetNoSendSheduleGroup(IEnumerable<SheduleData> current, IEnumerable<SheduleData> prev)
        {
            logger.LogInformation("Start GetNoSendSheduleGroup");

            var result = new List<int>();
            try
            {
                foreach (var sheduleCurrent in current)
                {
                    var shedulePrev = prev.FirstOrDefault(x => x.Group == sheduleCurrent.Group);

                    if (string.Equals(
                                    shedulePrev?.SheduleString,
                                    sheduleCurrent.SheduleString,
                                    StringComparison.InvariantCultureIgnoreCase))
                        result.Add(sheduleCurrent.Group);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetNoSendSheduleGroup");
            }

            return result;
        }

        private IEnumerable<SheduleData> GetSheduleDescription(IEnumerable<SheduleData> shedules)
        {
            var result = new List<SheduleData>();

            try
            {
                foreach (var sheduleCurrent in shedules)
                {
                    var sb = new StringBuilder();
                    var h = 0;
                    bool isOpenD = false;
                    foreach (var s in sheduleCurrent.SheduleString)
                    {
                        if (s == '0' && !isOpenD)
                        {
                            sb.Append($"    {GetFormatedH(h)}:00 - ");
                            isOpenD = true;
                        }

                        if (s == '1' && isOpenD)
                        {
                            sb.AppendLine( $"{GetFormatedH(h)}:00");
                            isOpenD = false;
                        }

                        h++;
                    }

                    if (isOpenD)
                        sb.AppendLine("24");

                    if (sb.Length == 0)
                        sb.Append("    - не планується");

                    result.Add(new SheduleData() { Group = sheduleCurrent.Group, SheduleString = sb.ToString() });
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetSheduleDescription");
            }
            return result;
        }

        private string GetFormatedH(int h)
            => h < 10 ? $" {h}" : h.ToString();

        private IEnumerable<SheduleData> GetShedulesFromFile(string file)
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
