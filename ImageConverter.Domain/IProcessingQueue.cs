using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain
{
    public interface IProcessingQueue
    {
        ProcessingQueueItem AddQueueItem(QueueItem queueItem);
        void Clear();
        ICollection<ProcessingQueueItem> GetLastQueueItems();
    }
}