using SQLite;

namespace ImageConverter.Domain.Storage
{
    [Flags]
    public enum QueueItemState
    {
        Queued = 0,
        Processing = 1,
        Processed = 2,
        Ignored = 3,
        Error = 4,
    }

    public class QueueItem
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string BaseDirectory { get; set; } = string.Empty;
        [Indexed(Name = "FullPath", Unique = true)]
        public string FullPath { get; set; } = string.Empty;
        public byte State { get; set; }
    }
}
