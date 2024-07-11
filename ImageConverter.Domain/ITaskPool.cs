using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain
{
    public interface ITaskPool
    {
        int QueueLength { get; }

        void ClearQueue();
        Task ExecuteTasksAsync(Func<QueueItem, Task> task, CancellationToken cancellationToken);
    }
}