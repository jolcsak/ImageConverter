using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IImageConverter
    {
        long SumInputSize { get; }
        long SumOutputSize { get; }

        Task ConvertImage(string? imageDirectory, string imagePath, string[]? transformerKeys, MagickFormat outputFormat);
    }
}