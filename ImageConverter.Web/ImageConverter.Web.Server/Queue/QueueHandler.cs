using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;
using ImageConverter.Domain.Storage;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Web.Server.Queue
{
    public class QueueHandler : IQueueHandler
    {
        private readonly ImageConverterConfiguration configuration;
        private readonly IStorageContext storageContext;
        private readonly IProcessingQueue processingQueue;
        private readonly ILogger<QueueHandler> logger;

        private readonly object _lock = new();

        public QueueHandler(
            IConfigurationHandler configurationHandler,
            IStorageContext storageContext,
            IProcessingQueue processingQueue,
            ILogger<QueueHandler> logger)
        {
            configuration = configurationHandler.GetConfiguration();
            this.storageContext = storageContext;
            this.logger = logger;
            this.processingQueue = processingQueue;
        }

        public int Length => storageContext.QueueItemRepository.Length;

        public void Enqueue(CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing queue");

            long newItemCount = 0;
            using (var dbContext = storageContext.CreateTransaction()) 
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

                        if (IsProcessable(filePath) && dbContext.QueueItemRepository.IsNotInQueue(filePath))
                        {
                            IQueueItem queueItem = new QueueItem
                            {
                                BaseDirectory = imageDirectory,
                                FullPath = filePath
                            };  
                            dbContext.QueueItemRepository.Enqueue(queueItem);
                            logger.LogInformation("New item added to the queue: {fullPath}", queueItem.FullPath);
                            newItemCount++;
                        }
                    }
                }
            }

            logger.LogInformation("Processing queue done");
        }

        public bool TryDequeue(out IQueueItem? queueItem)
        {
            return storageContext.QueueItemRepository.TryDequeue(out queueItem);
        }

        public void ClearQueue()
        {
            storageContext.QueueItemRepository.ClearQueue();
        }

        private bool IsProcessable(string filePath)
        {
            return configuration.SearchPattern!.Any(f => filePath.EndsWith("." + f, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
