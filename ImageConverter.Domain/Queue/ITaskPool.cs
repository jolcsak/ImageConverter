using ImageConverter.Domain.Storage;

namespace ImageConverter.Domain.Queue
{
    public interface ITaskPool
    {
        int QueueLength { get; }

        void ClearQueue();
        void CollectTasks(CancellationToken cancellationToken);
        Task ExecuteTasksAsync(Func<QueueItem, Task> task, CancellationToken cancellationToken);
    }
}