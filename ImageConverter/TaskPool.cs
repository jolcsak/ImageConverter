using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.QueueHandler;

namespace ImageConverter
{
    public class TaskPool : ITaskPool
    {
        private readonly IQueueHandler queueHandler;

        private readonly int maxDegreeOfParallelism;

        public TaskPool(IConfigurationHandler configurationHandler, IQueueHandler queueHandler)
        {
            this.queueHandler = queueHandler;
            var configuration = configurationHandler.GetConfiguration();
            maxDegreeOfParallelism = configuration.ThreadNumber!.Value;
        }

        public int QueueLength => queueHandler.Length;

        public void ClearQueue()
        {
            queueHandler.ClearQueue();
        }

        public async Task ExecuteTasksAsync(Func<QueueItem, Task> task, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested && queueHandler.TryDequeue(out var queueItem))
                    {
                        await task(queueItem!);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }
}

