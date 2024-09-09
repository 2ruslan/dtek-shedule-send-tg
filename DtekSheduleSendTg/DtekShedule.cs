using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.Shedule;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace DtekSheduleSendTg
{
    public class DtekShedule(ILogger logger, ISheduleRepository repository) : IDtekShedule
    {
        private readonly Rgba32 ColorWhite = new Rgba32(255, 255, 255, 255);

        private IEnumerable<int> noSendSheduleGroup;
        private IEnumerable<SheduleData> sheduleGroupDescription;


        public bool AnalyzeFile(string file)
        {
            var currentShedule = GetShedulesFromFile(file);
            var prevShedule = repository.GetShedule();

            noSendSheduleGroup = GetNoSendSheduleGroup(currentShedule, prevShedule);

            sheduleGroupDescription = GetSheduleDescription(currentShedule);

            repository.StoreShedule(currentShedule);

            return true;
        }

        public bool IsNoSendPicture2Group(long group)
            => noSendSheduleGroup.Any(x => x == group);

        public string GetFullPictureDescription(long group, string firsttLine)
        {
            var description = sheduleGroupDescription.FirstOrDefault(x => x.Group == group)?.SheduleString;

            var sb = new StringBuilder(firsttLine);
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine();
                sb.AppendLine("Відключення:");
                sb.Append(description);
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
                            sb.Append($"    {h} - ");
                            isOpenD = true;
                        }

                        if (s == '1' && isOpenD)
                        {
                            sb.AppendLine(h.ToString());
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

        private IEnumerable<SheduleData> GetShedulesFromFile(string file)
        {
            logger.LogInformation("Start GetShedules");

            var result = new List<SheduleData>();

            try
            {
                var img = Image.Load<Rgba32>(file);

                int group = 1;

                for (int g = 240; g < 490; g += 45)
                {
                    var sb = new StringBuilder();

                    for (int t = 164; t < 1000; t += 36)
                    {
                        var colot = img[t, g];
                        sb.Append(colot == ColorWhite ? "1" : "0");
                    }

                    result.Add(new SheduleData() { Group = group++, SheduleString = sb.ToString() });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetShedulesFromFile");
            }

            return result;
        }
    }
}
