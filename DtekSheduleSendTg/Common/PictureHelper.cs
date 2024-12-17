using Common;
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

        public static DateOnly GetDate(string file, ILogger logger)
        {
            string tesseractDir = Path.Combine(Environment.CurrentDirectory, "WorkDir", "Tesseract");

            try
            {
                var image = Image.Load<Rgba32>(file);
                var dim = GetDataParamsFormImage(logger, image);

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

                            if (str.Length > 7)
                            {
                                return new DateOnly(
                                    int.Parse(str.Substring(4, 4)),
                                    int.Parse(str.Substring(2, 2)),
                                    int.Parse(str.Substring(0, 2))
                                    );
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

            var nextB = GetNext(img, leftBorder.x + 10, leftBorder.y, Direction.bottom);
            var prevB = GetNext(img, leftBorder.x + 10, leftBorder.y, Direction.top);

            var yStep = nextB.y - prevB.y;
            var yStart = prevB.y - (yStep * 5) + (yStep / 2);

            for (int i = 0; i < GroupHelper.Groups.Length; i++)
                GroupCoord.Add(yStart + (i * yStep));

            // ----------------------------- x (group)
            x = img.Width - 1;
            y = yStart;

            var rightBorderBe = GetNext(img, x, y, Direction.left);

            var topCaptBorder = GetNext(img, rightBorderBe.x, rightBorderBe.y, Direction.top);
            var topBorderBs = GetNext(img, topCaptBorder.x - 5, topCaptBorder.y, Direction.bottom);

            var tlast = GetNext(img, topBorderBs.x, topBorderBs.y - 2, Direction.left);
            var first = (tlast.x, tlast.y);

            for (int i = 0; i < 46; i++)
                first = GetNext(img, first.x, first.y, Direction.left);

            for (int i = 0; i < 24; i++)
            {
                first = GetNext(img, first.x, first.y, Direction.right);
                var start = first.x;
                first = GetNext(img, first.x, first.y, Direction.right);
                var p = start + (first.x - start) / 2;
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
            var leftBottom = GetNext(img, leftTop.x, leftTop.y, Direction.bottom);
            var rightBottom = GetNext(img, leftBottom.x, leftBottom.y, Direction.right);
            rightBottom = GetNext(img, rightBottom.x, rightBottom.y, Direction.right);

            return new DtekDateFromFilePrarms()
            {
                StartX = leftTop.x,         // 613
                StartY = leftTop.y,         // 41
                FinishX = rightBottom.x,    // 727
                FinishY = rightBottom.y     // 60
            };
        }

        private static (int x, int y) GetNext(Image<Rgba32> img, int x, int y, Direction direction)
        {
            var startPixel = img[x, y];
            var startAvg = (int)Math.Round(.299 * startPixel.R + .587 * startPixel.G + .114 * startPixel.B); 

            var nextAvg = startAvg;
            while (Math.Abs(nextAvg - startAvg) < 15)
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

              //  Console.WriteLine($"{nextAvg} {startAvg}"); 
            }

            return (x, y);
        }

    }
}
