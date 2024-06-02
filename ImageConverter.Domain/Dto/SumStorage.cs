using SQLite;

namespace ImageConverter.Domain.Dto
{
    public class SumStorage
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public long ProcessedBytes { get; set; }
        public long SumSavedBytes { get; set; }
        public long ConvertedImageCount { get; set; }
        public long DeletedFileCount { get; set; }
        public long SumDeleteFileSize { get; set; }
        public long LastStarted { get; set; }
        public long LastFinished { get; set; }
        public long NextFire { get; set; }
        public string? State { get; set; }
    }
}
