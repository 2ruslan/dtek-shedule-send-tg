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
        private const int _COLOR_ON05START = -1;
        private const int _COLOR_ON05FINISH = -2;
        public string GetSchedule(string group)
            => currentShedule.FirstOrDefault(x => x.GroupNum == group)?.SheduleString;
        
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

        public string GetFullPictureDescription(string group, string firsttLine, string linePatern, string leadingSymbol)
        {
            var sheduleString = currentShedule.FirstOrDefault(x => x.GroupNum == group)?.SheduleString ?? string.Empty;

            var sb = new StringBuilder();
            if (!string.Equals(GroupHelper.AllGroups, group))
            {
                try
                {
                    var h = 0;
                    bool isOpenD = false;
                    var startPeriodH = 0;
                    var startPeriodM = 0;

                    foreach (var s in sheduleString)
                    {
                        if (s == SheduleData._OFF && !isOpenD)
                        {
                            startPeriodH = h;
                            startPeriodM = 0;
                            isOpenD = true;
                        }
                        else if (s == SheduleData._ON_05_START && !isOpenD)
                        {
                            startPeriodH = h;
                            startPeriodM = 30;
                            isOpenD = true;
                        }
                        else if (s == SheduleData._ON && isOpenD)
                        {
                            sb.AppendLine(TextHelper.GetFomatedLine(linePatern, leadingSymbol,
                                                                    startPeriodH, startPeriodM,
                                                                    h, 0));
                            isOpenD = false;
                        }
                        else if (s == SheduleData._ON_05_END && isOpenD)
                        {
                            sb.AppendLine(TextHelper.GetFomatedLine(linePatern, leadingSymbol,
                                                                    startPeriodH, startPeriodM,
                                                                    h, 30));
                            isOpenD = false;
                        }

                        h++;
                    }

                    if (isOpenD)
                        sb.AppendLine(TextHelper.GetFomatedLine(linePatern, leadingSymbol,
                                                                startPeriodH, startPeriodM,
                                                                24, 0));

                    if (sb.Length == 0)
                        sb.Append("<b>    - не планується</b>");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "GetSheduleDescription");
                }
            }

            if (sb.Length > 0)
                sb.Insert(0, "\r\n");

            sb.Insert(0,firsttLine);

            return sb.ToString();
        }

        private IList<SheduleData> GetShedulesFromFile(string file)
        {
            logger.LogInformation("Start GetShedules");

            var result = new List<SheduleData>();

            try
            {
                var img = SixLabors.ImageSharp.Image.Load<Rgba32>(file);

                var imgP = PictureHelper.ApplyGammaCorrection(img, 15);
                imgP = PictureHelper.ConvertToBlackAndWhite(imgP);
                //imgP.Save(@"c:\temp\123.jpg");
                var dim = PictureHelper.GetGridParamsFormImage(logger, imgP);
                
                int group = 1;
                
                foreach (var g in dim.GroupCoord)
                {
                    var sb = new StringBuilder();
                    Console.WriteLine(GroupHelper.DtekPositions[group]);
                    foreach(var t in dim.TimeCoord)
                    {
                        int average = GetAverage(img, t, g);
                        
                        char sSymbol;
                        
                        if (average == _COLOR_ON05START) 
                            sSymbol = SheduleData._ON_05_START;
                        else if (average == _COLOR_ON05FINISH)
                            sSymbol = SheduleData._ON_05_END;
                        else if (average > 200) 
                            sSymbol = SheduleData._ON;
                        else 
                            sSymbol = SheduleData._OFF;

                        sb.Append(sSymbol);
                    }
                    
                    result.Add(new SheduleData() { 
                                        GroupNum = GroupHelper.DtekPositions[group++], 
                                        SheduleString = sb.ToString() 
                            });
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
            
            var tm = new int[] { t - 3, t - 2, t - 1, t, t + 1, t + 2, t + 3 };
            var gm = new int[] {g - 1, g, g + 1 };
            
            var qnt = tm.Length * gm.Length;

            foreach (var tp in tm)
                foreach (var gp in gm)
                {
                    var colorPixel = img[tp, gp];
                    
                    if (colorPixel.R > 200 && colorPixel.G > 200 && colorPixel.B < 200)
                        return tp < t ?  _COLOR_ON05START : _COLOR_ON05FINISH;

                    average += (((colorPixel.R + colorPixel.B + colorPixel.G) / 3.0) / qnt);
                }

            return (int)average;
        }
    }

}
