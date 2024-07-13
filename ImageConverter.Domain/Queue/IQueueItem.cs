namespace ImageConverter.Domain.Queue
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

    public interface IQueueItem
    {
        string BaseDirectory { get; set; }
        string FullPath { get; set; }
        long Id { get; set; }
        byte State { get; set; }
    }
}