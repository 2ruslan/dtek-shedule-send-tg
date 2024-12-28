using Common;
using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;
using Tesseract;

namespace DtekSheduleSendTg.DTEK
{
    public static class PictureHelper
    {
        private enum Direction
        {
            left, right, top, bottom
        }

        public static DateOnly GetDate(string file, ILogger logger, IMonitoring monitoring)
        {
            logger.LogInformation(">>>GetDate from file");

            string tesseractDir = Path.Combine(Environment.CurrentDirectory, "WorkDir", "Tesseract");

            try
            {
                var image = Image.Load<Rgba32>(file);
                var dim = GetDataParamsFormImage(logger, image);
                
                logger.LogInformation("dim {0} {1} {2} {3}", dim.StartX, dim.StartY, dim.FinishX, dim.FinishY);

                Rectangle cropArea = new Rectangle(dim.StartX, dim.StartY, dim.FinishX - dim.StartX, dim.FinishY - dim.StartY);
                image.Mutate(x => x.Crop(cropArea));

                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, new PngEncoder()); 
                    byte[] imageBytes = ms.ToArray();

                    var engine = new TesseractEngine(tesseractDir, "eng", EngineMode.Default);

                    using (var pix = Pix.LoadFromMemory(imageBytes))
                    {
                        using (var page = engine.Process(pix))
                        {
                            var str = page.GetText()
                                            .Trim()
                                            .Replace(" ", string.Empty)
                                            .Replace(":", string.Empty)
                                            .Replace(".", string.Empty)
                                            .Replace("-", string.Empty)
                                            ;
                            logger.LogInformation("str {0}", str);
                            
                            monitoring.Append("parsed dt", str);

                            if (str.Length > 7)
                            {
                                var candidateDT = new DateTime(
                                                int.Parse(str.Substring(4, 4)),
                                                int.Parse(str.Substring(2, 2)),
                                                int.Parse(str.Substring(0, 2))
                                                );

                                if (Math.Abs((candidateDT - DateTime.Now).TotalDays) < 3)
                                    return DateOnly.FromDateTime(candidateDT);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetDate");
            }

            return DateOnly.MinValue;
        }


        public static DtekShedulesFromFilePrarms GetGridParamsFormImage(ILogger loger, Image<Rgba32> img)
        {
            var TimeCoord = new List<int>();
            var GroupCoord = new List<int>();
            int x, y = 0;

            // ----------------------------- y (group)
            x = 0;
            y = img.Height / 2;

            var leftBorder = GetNext(img, x, y, Direction.right);

            var firstG = (x: leftBorder.x + 10, y: leftBorder.y);
            for (int i = 0; i < 11; i++)
                firstG = GetNext(img, firstG.x, firstG.y, Direction.top);

            var lastG = (x: firstG.x, y: firstG.y);
            for (int i = 0; i < 12; i++)
            {
                lastG = GetNext(img, lastG.x, lastG.y, Direction.bottom);
                var start = lastG.y;
                lastG = GetNext(img, lastG.x, lastG.y, Direction.bottom);
                var p = start + (lastG.y - start) / 2;
                GroupCoord.Add(p);
            }


            // ----------------------------- x (group)
            x = img.Width - 1;
            y = GroupCoord[0];

            var rightBorderBe = GetNext(img, x, y, Direction.left);

            var topCaptBorder = GetNext(img, rightBorderBe.x, rightBorderBe.y, Direction.top);
            var topBorderBs = GetNext(img, topCaptBorder.x - 5, topCaptBorder.y, Direction.bottom);

            var tlast = GetNext(img, topBorderBs.x, topBorderBs.y - 4, Direction.left);
            var first = (tlast.x, tlast.y);

            for (int i = 0; i < 46; i++)
                first = GetNext(img, first.x, first.y, Direction.left);

            for (int i = 0; i < 24; i++)
            {
                first = GetNext(img, first.x, first.y, Direction.right);
                var start = first.x;
                first = GetNext(img, first.x, first.y, Direction.right);
                var p = start + (first.x - start) / 2;
                
           //     Console.WriteLine($"----------------------- {p}");

                TimeCoord.Add(p);
            }

            StringBuilder sb = new StringBuilder();
            foreach(var g in GroupCoord)
                sb.Append($"{g}, ");
            loger.LogInformation("GroupCoord = {0}", sb);

            sb.Clear();
            foreach (var t in TimeCoord)
                sb.Append($"{t}, ");
            loger.LogInformation("TimeCoord = {0}", sb);

            if (GroupCoord.Count != GroupHelper.Groups.Length || TimeCoord.Count != 24)
            {
                GroupCoord.Clear();
                TimeCoord.Clear();
                loger.LogWarning("Clear TimeCoord&GroupCoord");
            }

            return new DtekShedulesFromFilePrarms()
            {
                GroupCoord = GroupCoord,
                TimeCoord = TimeCoord
            };
        }

        public static DtekDateFromFilePrarms GetDataParamsFormImage(ILogger loger, Image<Rgba32> img)
        {
            var x = (int)(img.Width * 0.66);
            var y = 2;

            var topO = GetNext(img, x, y, Direction.bottom);
            var topI = GetNext(img, topO.x, topO.y, Direction.bottom);
            var leftTop = GetNext(img, topI.x, topI.y, Direction.left);
            leftTop = GetNext(img, leftTop.x, leftTop.y, Direction.bottom);
           // var leftBottom = GetNext(img, leftTop.x, leftTop.y, Direction.bottom);
           // var rightBottom = GetNext(img, leftBottom.x, leftBottom.y, Direction.right);
           // rightBottom = GetNext(img, rightBottom.x, rightBottom.y, Direction.right);

            return new DtekDateFromFilePrarms()
            {
                StartX = leftTop.x,         // 613
                StartY = leftTop.y,         // 41
                FinishX = leftTop.x + 110,  // 727
                FinishY = leftTop.y + 30    // 60
            };
        }

        private static (int x, int y) GetNext(Image<Rgba32> img, int x, int y, Direction direction)
        {
            var startPixel = img[x, y];
            var startAvg = (int)Math.Round(.299 * startPixel.R + .587 * startPixel.G + .114 * startPixel.B); 

            var nextAvg = startAvg;
            while (Math.Abs(nextAvg - startAvg) < 30)
            {
                switch (direction)
                {
                    case Direction.right: x++; break;
                    case Direction.left: x--; break;
                    case Direction.top: y--; break;
                    case Direction.bottom: y++; break;
                }

                if (x < 0 || y < 0 || x >= img.Width || y >= img.Height)
                    throw new Exception(":(");
                var nextPixel = img[x, y];

                nextAvg = (int)Math.Round(.299 * nextPixel.R + .587 * nextPixel.G + .114 * nextPixel.B);

              //  Console.WriteLine($"{nextAvg - startAvg}"); 
            }

            return (x, y);
        }


        public static Image<Rgba32> ApplyGammaCorrection(Image<Rgba32> original, double gamma)
        {
            if (gamma <= 0)
                throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be greater than 0.");

            var correctedImage = new Image<Rgba32>(original.Width, original.Height);

            byte[] gammaCorrectionTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                gammaCorrectionTable[i] = (byte)Math.Min(255, (int)(255 * Math.Pow(i / 255.0, gamma)));
            }

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    var originalColor = original[x, y];

                    var correctedColor = Color.FromRgb(
                        gammaCorrectionTable[originalColor.R],
                        gammaCorrectionTable[originalColor.G],
                        gammaCorrectionTable[originalColor.B]
                    );

                    correctedImage[x, y] = correctedColor;
                }
            }

            return correctedImage;
        }

        public static Image<Rgba32> ConvertToBlackAndWhite(Image<Rgba32> originalImage)
        {
            var blackAndWhiteImage = new Image<Rgba32>(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    var originalColor = originalImage[x, y];

                    int brightness = (originalColor.R + originalColor.G + originalColor.B) / 3;

                    var newColor = brightness < 100 ? Color.Black : Color.White;

                    blackAndWhiteImage[x, y] = newColor;
                }
            }

            return blackAndWhiteImage;
        }
    }
}
