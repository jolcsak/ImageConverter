using ImageConverter.Domain.Queue;
using SQLite;

namespace ImageConverter.Storage.Entities
{
    public class QueueItem : IQueueItem
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string BaseDirectory { get; set; } = string.Empty;
        [Indexed(Name = "FullPath", Unique = true)]
        public string FullPath { get; set; } = string.Empty;
        public byte State { get; set; }
    }
}
