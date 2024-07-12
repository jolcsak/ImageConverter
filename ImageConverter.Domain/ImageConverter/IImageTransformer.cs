using ImageMagick;

namespace ImageConverter.Domain.ImageConverter
{
    public interface IImageTransformer
    {
        public string Key { get; }
        public void TransformImage(MagickImage image);
    }
}
