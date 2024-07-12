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
        private readonly IProcessingQueue processingQueue;
        private readonly ILogger<QueueHandler> logger;

        private object _lock = new();

        public QueueHandler(
            IConfigurationHandler configurationHandler,
            IStorageHandler storageHandler,
            IProcessingQueue processingQueue,
            ILogger<QueueHandler> logger)
        {
            configuration = configurationHandler.GetConfiguration();
            this.storageHandler = storageHandler;
            this.logger = logger;
            this.processingQueue = processingQueue;
        }

        public int Length
        {
            get
            {
                using (var db = storageHandler.GetConnection())
                {
                    return db.Table<QueueItem>().Count(qi => qi.State == (byte)QueueItemState.Queued);
                }
            }
        }

        public void Enqueue(CancellationToken cancellationToken)
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

                        FileInfo fileInfo = new FileInfo(filePath);
                        processingQueue.AddProcessingPath(fileInfo.DirectoryName);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        if (IsProcessable(filePath) && IsNotInQueue(db, filePath))
                        {
                            QueueItem queue = new QueueItem { 
                                BaseDirectory = imageDirectory, 
                                FullPath = filePath, 
                                State = (byte)QueueItemState.Queued
                            };
                            db.Insert(queue);
                            logger.LogInformation("New item added to the queue: {fullPath}", queue.FullPath);
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

        public bool TryDequeue(out QueueItem? queueItem)
        {
            lock (_lock)
            {
                using (var db = storageHandler.GetConnection())
                {
                    var nextItem = db.Table<QueueItem>().FirstOrDefault(qi => qi.State == (byte)QueueItemState.Queued);
                    queueItem = nextItem;
                    if (queueItem != null)
                    {
                        queueItem.State = (byte)QueueItemState.Processing;
                        db.Update(queueItem);
                        return true;
                    }
                    return false;
                }
            }
        }

        public void ClearQueue()
        {
            storageHandler.ClearQueue();
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
