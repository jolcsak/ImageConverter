using ImageMagick;

namespace ImageConverter.Domain
{
    public interface IImageConverter
    {
        Task<long?> ConvertImage(string basePath, FileInfo inputFileInfo, string[]? transformerKeys, MagickFormat outputFormat);
    }
}