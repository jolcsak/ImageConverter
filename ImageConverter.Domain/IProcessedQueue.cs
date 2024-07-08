using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain
{
    public interface IProcessedQueue
    {
        void AddQueueItem(QueueItem queueItem);
        void Clear();
        ICollection<QueueItem> GetLastQueueItems();
    }
}