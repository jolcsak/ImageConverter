﻿using ImageConverter.Domain;
using ImageConverter.Domain.Queue;
using System.Runtime.CompilerServices;

namespace ImageConverter.Web.Server.Queue
{
    public class TaskPool : ITaskPool
    {
        private readonly IQueueHandler queueHandler;
        private readonly IExecutionContext executionContext;
        private readonly ILogger<TaskPool> logger;

        private readonly int maxDegreeOfParallelism;

        public TaskPool(
            IConfigurationHandler configurationHandler,
            IQueueHandler queueHandler,
            IExecutionContext executionContext,
            ILogger<TaskPool> logger)
        {
            this.queueHandler = queueHandler;
            this.executionContext = executionContext;
            this.logger = logger;

            var configuration = configurationHandler.GetConfiguration();
            maxDegreeOfParallelism = configuration.ThreadNumber!.Value;
        }

        public int QueueLength => queueHandler.Length;

        public void ClearQueue()
        {
            queueHandler.ClearQueue();
        }

        public void CollectTasks(CancellationToken cancellationToken)
        {
            executionContext.ExecutionState = ExecutionState.Collecting;
            queueHandler.Enqueue(cancellationToken);
        }

        public async Task ExecuteTasksAsync(Func<IQueueItem, Task> task, CancellationToken cancellationToken)
        {
            executionContext.ExecutionState = ExecutionState.Compressing;

            try
            {
                var tasks = new List<Task>();

                for (int i = 0; i < maxDegreeOfParallelism; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested && queueHandler.TryDequeue(out var queueItem))
                        {
                            try
                            {
                                await task(queueItem!);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error during image conversion!");
                            }
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                executionContext.ExecutionState = ExecutionState.Done;
            }
        }
    }
}

