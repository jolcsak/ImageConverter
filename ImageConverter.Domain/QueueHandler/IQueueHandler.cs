using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain.QueueHandler
{
    public interface IQueueHandler
    {
        void Enqueue();
        Task DequeueAsync(Func<QueueItem, Task> task);
    }
}