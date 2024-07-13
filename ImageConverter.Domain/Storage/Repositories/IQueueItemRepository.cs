using ImageConverter.Domain.Queue;

namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IQueueItemRepository
    {
        int Length { get; }

        void Update(IQueueItem? queueItem);
        void Delete(IQueueItem? queueItem);

        void Enqueue(IQueueItem queueItem);
        bool TryDequeue(out IQueueItem? queueItem);

        public bool IsNotInQueue(string filePath);
        void ClearQueue();
    }
}
