using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;

namespace ImageConverter.Web.Server
{
    public class ProcessingQueue : IProcessingQueue
    {
        private const uint MaxSize = 20;
        private readonly ThreadSafeList<ProcessingQueueItem> processedQueue = new();

        public ProcessingQueueItem AddQueueItem(QueueItem queueItem)
        {
            var newItem = new ProcessingQueueItem(queueItem, ProcessingQueueItemState.Compressing);
            processedQueue.Add(newItem);
            if (processedQueue.Count > MaxSize)
            {
                processedQueue.RemoveAt(0);
            } 
            return newItem;
        }

        public void Clear()
        {
            processedQueue.Clear();
        }

        public ICollection<ProcessingQueueItem> GetLastQueueItems()
        {
            return processedQueue.ToList();
        }
    }
}
