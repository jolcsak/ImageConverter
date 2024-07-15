using ImageConverter.Domain.Queue;

namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IQueueItemRepository
    {
        int Length { get; }

        void Update(IQueueItem? queueItem);
        void Delete(IQueueItem? queueItem);

        void Enqueue(string baseDirectory, string filePath);
        bool TryDequeue(out IQueueItem? queueItem);

        public bool IsNotInQueue(string filePath);
        void ClearQueue();
    }
}
