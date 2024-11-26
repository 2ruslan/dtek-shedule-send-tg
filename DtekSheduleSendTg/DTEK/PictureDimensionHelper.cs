using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace DtekSheduleSendTg.DTEK
{
    public static class PictureDimensionHelper
    {
        private enum Direction
        {
            left, right, top, bottom
        }

        public static DtekShedulesFromFilePrarms GetParamsFormImage(ILogger loger, Image<Rgba32> img)
        {
            var TimeCoord = new List<int>();
            var GroupCoord = new List<int>();
            int x, y = 0;

            // ----------------------------- y (group)
            x = 0;
            y = img.Height / 2;

            var leftBorder = GetNext(img, x, y, Direction.right);

            var bottomBorder = GetNext(img, leftBorder.x, leftBorder.y, Direction.bottom);

            var g56be = GetNext(img, bottomBorder.x + 5, bottomBorder.y - 5, Direction.top);
            var g56bs = GetNext(img, g56be.x, g56be.y, Direction.top);

            var g45be = GetNext(img, g56bs.x, g56bs.y, Direction.top);

            var yStep = g56be.y - g45be.y;
            var yStart = g45be.y - (yStep * 4) + (yStep / 2);

            for (int i = 0; i < 6; i++)
                GroupCoord.Add(yStart + (i * yStep));

            Console.WriteLine($"{yStart} {yStep}");

            // ----------------------------- x (group)
            x = img.Width - 1;
            y = yStart;

            var rightBorderBe = GetNext(img, x, y, Direction.left);
            var rightBorderBs = GetNext(img, rightBorderBe.x, rightBorderBe.y, Direction.left);

            var topBorderBe = GetNext(img, rightBorderBs.x, rightBorderBs.y, Direction.top);
            var topBorderBs = GetNext(img, topBorderBe.x, topBorderBe.y, Direction.top);

            var tlast = GetNext(img, topBorderBs.x, topBorderBs.y, Direction.left);
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

            if (GroupCoord.Count != 6 || TimeCoord.Count != 24)
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
       
        private static (int x, int y) GetNext(Image<Rgba32> img, int x, int y, Direction direction)
        {
            var startPixel = img[x, y];
            var startAvg = (startPixel.R + startPixel.B + startPixel.G) / 3;

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
                nextAvg = (nextPixel.R + nextPixel.B + nextPixel.G) / 3;
                Math.Abs(nextAvg - startAvg);
            }

            return (x, y);
        }

    }
}
