using SQLite;

namespace ImageConverter.Domain.DbEntities
{
    [Flags]
    public enum QueueItemState
    {
        Queued = 0,
        Processed = 1,
        Ignored = 2,
        Error = 4
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
