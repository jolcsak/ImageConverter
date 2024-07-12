using ImageConverter.Domain.Storage;

namespace ImageConverter.Domain.Queue
{
    public interface IQueueHandler
    {
        int Length { get; }
        void Enqueue(CancellationToken cancellationToken);
        bool TryDequeue(out QueueItem? queueItem);
        void ClearQueue();
    }
}