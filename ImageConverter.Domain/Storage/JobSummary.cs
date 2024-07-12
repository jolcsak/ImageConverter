using SQLite;

namespace ImageConverter.Domain.Storage
{
    public class JobSummary
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public DateTime JobStarted { get; set; }
        public DateTime? JobFinished { get; set; }
        public long InputBytes { get; set; }
        public long OutputBytes { get; set; }
        public long ConvertedImageCount { get; set; }
        public long ErrorCount { get; set; }
        public long DeletedFileCount { get; set; }
        public long IgnoredFileCount { get; set; }
        public long SumDeletedFileSize { get; set; }
        public string? State { get; set; }
    }
}
