using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.QueueHandler;
using Microsoft.Extensions.Logging;
using SQLite;

namespace ImageConverter.QueueHandler
{
    public class QueueHandler : IQueueHandler
    {
        private const uint BatchSize = 100;

        private readonly ImageConverterConfiguration configuration;
        private readonly IStorageHandler storageHandler;
        private readonly ILogger<QueueHandler> logger;
        private readonly ITaskPool taskPool;

        public QueueHandler(
            IConfigurationHandler configurationHandler,
            IStorageHandler storageHandler, 
            ILogger<QueueHandler> logger,
            ITaskPool taskPool)
        {
            configuration = configurationHandler.GetConfiguration();
            this.storageHandler = storageHandler;
            this.logger = logger;
            this.taskPool = taskPool;
        }

        public void Enqueue()
        {
            logger.LogInformation("Processing queue");

            long newItemCount = 0;
            using (var db = storageHandler.GetConnection())
            {
                foreach (string? imageDirectory in configuration.ImageDirectories!)
                {
                    logger.LogInformation("Getting files for : {dir}", imageDirectory);
                    string[] files = Directory.GetFiles(imageDirectory!, "*", SearchOption.AllDirectories);
                    foreach (string filePath in files)
                    {
                        if (IsProcessable(filePath) && IsNotInQueue(db, filePath))
                        {
                            QueueItem queue = new QueueItem { 
                                BaseDirectory = imageDirectory, 
                                FullPath = filePath, 
                                State = (byte)QueueItemState.Queued 
                            };
                            db.Insert(queue);
                            logger.LogInformation($"New item added to the queue: {queue.FullPath}");
                            newItemCount++;

                            if (newItemCount % BatchSize == 0)
                            {
                                db.Commit();
                            }
                        }
                    }
                }
            }

            logger.LogInformation("Processing queue done");
        }

        public async Task DequeueAsync(Func<QueueItem, Task> task, CancellationToken cancellationToken)
        {
            using (var db = storageHandler.GetConnection())
            {
                foreach (QueueItem queueItem in db.Table<QueueItem>().Where(q => q.State == (byte)QueueItemState.Queued))
                {
                    taskPool.EnqueueTask(() => task(queueItem));
                }
            }

            await taskPool.ExecuteTasksAsync(cancellationToken);
        }

        private bool IsProcessable(string filePath)
        {
            return configuration.SearchPattern!.Any(f => filePath.EndsWith("." + f, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsNotInQueue(SQLiteConnection db, string filePath)
        {
            return !db.Table<QueueItem>().Any(q => q.FullPath == filePath);
        }
    }
}
