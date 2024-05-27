namespace ImageConverter.Domain.Dto
{
    public class SumStorage
    {
        public long ProcessedBytes { get; set; }
        public long SumSavedBytes { get; set; }
        public long ConvertedImageCount { get; set; }
        public long DeletedFileCount { get; set; }
        public long SumDeleteFileSize { get; set; }
    }
}
