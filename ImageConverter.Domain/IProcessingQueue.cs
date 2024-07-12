using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain
{
    public interface IProcessingQueue
    {
        void AddProcessingPath(string? processingPath);
        ProcessingQueueItem AddQueueItem(QueueItem queueItem);
        ICollection<string> GetLastProcessingPaths();
        ICollection<ProcessingQueueItem> GetLastQueueItems();

        void ClearQueue();
    }
}