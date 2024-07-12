using ImageConverter.Domain.ImageConverter;
using ImageMagick;

namespace ImageConverter.Transformers
{
    public class ImageResizeTransformer : IImageTransformer
    {
        private const int maxWidth = 8000;
        private const int maxHeight = 5000;

        public string Key => nameof(ImageResizeTransformer);

        public void TransformImage(MagickImage image)
        {
            if (image.Width > maxWidth && image.Width > image.Height)
            {
                image.Resize(maxWidth, Convert.ToInt32(image.Height * maxWidth / image.Width));
            }
            else if (image.Height > maxHeight)
            {
                image.Resize(Convert.ToInt32(image.Width * maxHeight / image.Height), maxHeight);
            }
        }
    }
}
