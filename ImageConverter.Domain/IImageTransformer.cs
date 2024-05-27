using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IImageTransformer
    {
        public string Key { get; }
        public void TransformImage(MagickImage image);
    }
}
