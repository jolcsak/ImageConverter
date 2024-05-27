using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IConversionRule
    {
        public MagickFormat ImageFormat { get; }

        public bool SetImage(MagickImage image);
    }
}
