namespace ImageConverter.Domain
{
    public interface ITaskPool
    {
        void EnqueueTask(Func<Task> task);
        Task ExecuteTasksAsync();
    }
}