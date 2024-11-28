using SixLabors.ImageSharp.PixelFormats;

namespace DtekSheduleSendTg.Common
{
    internal static class PictureHelper
    {

        public static Rgba32 GammaCorrection(this Rgba32 point, double gamma)
            => new Rgba32(  
                        (byte)Math.Pow(((double)point.R / 255), gamma) * 255,
                        (byte)Math.Pow(((double)point.G / 255), gamma) * 255,
                        (byte)Math.Pow(((double)point.B / 255), gamma) * 255,
                        point.A
                );

    }
}
