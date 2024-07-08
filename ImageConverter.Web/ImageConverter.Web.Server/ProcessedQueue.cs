using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using System.Collections.Concurrent;

namespace ImageConverter.Web.Server
{
    public class ProcessedQueue : IProcessedQueue
    {
        private readonly ConcurrentBag<QueueItem> processedQueue = new();

        public void AddQueueItem(QueueItem queueItem)
        {
            processedQueue.Add(queueItem);
        }

        public void Clear()
        {
            processedQueue.Clear();
        }

        public ICollection<QueueItem> GetLastQueueItems()
        {
            return processedQueue.OrderBy(q => q.Id).TakeLast(20).ToList();
        }
    }
}
