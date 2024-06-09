using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IImageConverter
    {
        Task<long?> ConvertImage(string? imageDirectory, string imagePath, string[]? transformerKeys, MagickFormat outputFormat);
    }
}