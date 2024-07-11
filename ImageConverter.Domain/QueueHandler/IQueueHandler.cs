using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain.QueueHandler
{
    public interface IQueueHandler
    {
        int Length { get; }
        void Enqueue(CancellationToken cancellationToken);
        bool TryDequeue(out QueueItem? queueItem);
        void ClearQueue();
    }
}