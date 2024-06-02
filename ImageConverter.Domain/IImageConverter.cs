using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IImageConverter
    {
        Task ConvertImage(string? imageDirectory, string imagePath, string[]? transformerKeys, MagickFormat outputFormat);
    }
}