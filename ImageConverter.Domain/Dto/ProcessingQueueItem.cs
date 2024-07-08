using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain.Dto
{
    public enum ProcessingQueueItemState
    {
        Compressing,
        Recompressing,
        Compressed,
        Ignored,
        Deleted,
        Failed
    }

    public class ProcessingQueueItem
    {
        public ProcessingQueueItemState State { get; set; }
        public long InputFileSize { get; set; }
        public long OutputFileSize { get; set; }
        public int Quality { get; set; }

        public QueueItem QueueItem { get; set; }

        public ProcessingQueueItem(QueueItem queueItem, ProcessingQueueItemState state)
        {
            State = state;
            QueueItem = queueItem;
        }
    }
}
