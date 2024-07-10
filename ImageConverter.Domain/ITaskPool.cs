namespace ImageConverter.Domain
{
    public interface ITaskPool
    {
        int QueueLength { get; }

        void ClearQueue();
        void EnqueueTask(Func<Task> task);
        Task ExecuteTasksAsync(CancellationToken cancellationToken);
    }
}