using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Dto;

namespace ImageConverter.Domain.Queue
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