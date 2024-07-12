using ImageConverter.Domain.Dto;
using ImageMagick;

namespace ImageConverter.Domain.ImageConverter
{
    public interface IImageConverter
    {
        Task<long?> ConvertImage(ProcessingQueueItem processingQueueItem, FileInfo inputFileInfo, string[]? transformerKeys, MagickFormat outputFormat);
    }
}