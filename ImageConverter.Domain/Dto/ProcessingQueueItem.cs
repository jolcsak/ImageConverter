using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain.Dto
{
    public enum ProcessingQueueItemState
    {
        Compressing,
        Recompressing,
        Deleted
    }

    public class ProcessingQueueItem
    {
        public ProcessingQueueItemState State { get; set; }
        public QueueItem QueueItem { get; set; }

        public ProcessingQueueItem(QueueItem queueItem, ProcessingQueueItemState state)
        {
            State = state;
            QueueItem = queueItem;
        }
    }
}
