using ImageConverter.Domain.Storage;
using SQLite;

namespace ImageConverter.Storage.Entities
{
    public class ImageConverterSummary : IImageConverterSummary
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public long InputBytes { get; set; }
        public long OutputBytes { get; set; }
        public long ConvertedImageCount { get; set; }
        public long ErrorCount { get; set; }
        public long DeletedFileCount { get; set; }
        public long IgnoredFileCount { get; set; }
        public long SumDeletedFileSize { get; set; }
        public DateTime LastStarted { get; set; }
        public DateTime? LastFinished { get; set; }
        public DateTime? NextFire { get; set; }
        public string? State { get; set; }
        public long JobCount { get; set; }
    }
}
