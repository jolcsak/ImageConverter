using ImageConverter.Domain.ImageConverter.ConversionRules;
using ImageMagick;

namespace ImageConverter.ConversionRules.In
{
    public class PngInRule : IConversionInRule
    {
        public MagickFormat ImageFormat => MagickFormat.Png;

        public bool SetImage(MagickImage image)
        {
            image.Quality = 85;
            return true;
        }
    }
}
