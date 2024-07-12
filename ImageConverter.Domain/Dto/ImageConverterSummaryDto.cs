using ImageConverter.Domain.Storage;

namespace ImageConverter.Domain.Dto
{
    public class ImageConverterSummaryDto : ImageConverterSummary
    {
        public DateTime CurrentDate { get; set; }
        public int MaxThreads { get; set; }
    }
}
