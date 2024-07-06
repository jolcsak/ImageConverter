using ImageConverter.Domain;
using System.Collections.Concurrent;

namespace ImageConverter
{
    public class TaskPool : ITaskPool
    {
        private readonly int maxDegreeOfParallelism;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Func<Task>> tasks;

        public TaskPool(IConfigurationHandler configurationHandler)
        {
            var configuration = configurationHandler.GetConfiguration();
            maxDegreeOfParallelism = configuration.ThreadNumber!.Value;
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
            tasks = new ConcurrentQueue<Func<Task>>();
        }

        public void EnqueueTask(Func<Task> task)
        {
            tasks.Enqueue(task);
        }

        public async Task ExecuteTasksAsync()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (this.tasks.TryDequeue(out var task))
                    {
                        await _semaphore.WaitAsync();
                        try
                        {
                            await task();
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}

