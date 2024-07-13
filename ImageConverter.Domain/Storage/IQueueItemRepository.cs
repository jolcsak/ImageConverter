
using ImageConverter.Domain.Queue;

namespace ImageConverter.Domain.Storage
{
    public interface IQueueItemRepository
    {
        int Length { get; }

        void Enqueue(IQueueItem  queueItem);
        bool TryDequeue(out IQueueItem? queueItem);

        public bool IsNotInQueue(string filePath);
        void ClearQueue();
    }
}
