using ImageMagick;

namespace ImageConverter.Domain.ImageConverter.ConversionRules
{
    public interface IConversionRule
    {
        public MagickFormat ImageFormat { get; }

        public bool SetImage(MagickImage image);
    }
}
