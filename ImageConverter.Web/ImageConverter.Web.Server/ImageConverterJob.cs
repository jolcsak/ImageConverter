using ImageConverter.Domain;
using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;
using Quartz;
using ImageConverter.Domain.ImageConverter;

namespace ImageConverter.Web.Server
{
    [DisallowConcurrentExecution]
    public class ImageConverterJob : IJob
    {
        private readonly IImageConverter imageConverter;
        private readonly IFileCleaner fileCleaner;
        private readonly ILogger<ImageConverterJob> logger;
        private readonly ImageConverterConfiguration configuration;
        private readonly IImageConverterJobHandler imageConverterContext;
        private readonly IProcessingQueue processingQueue;
        private readonly ITaskPool taskPool;

        public ImageConverterJob(
            IImageConverter imageConverter, 
            IFileCleaner fileCleaner,
            ILogger<ImageConverterJob> logger,
            IConfigurationHandler configurationHandler, 
            IImageConverterJobHandler imageConverterContext,
            IProcessingQueue processingQueue,
            ITaskPool taskPool)
        {
            this.imageConverter = imageConverter;
            this.fileCleaner = fileCleaner;
            this.logger = logger;
            configuration = configurationHandler.GetConfiguration();
            this.imageConverterContext = imageConverterContext;
            this.processingQueue = processingQueue;
            this.taskPool = taskPool;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Conversion started, using {threadCount} threads.", configuration.ThreadNumber!.Value);

            imageConverterContext.OnJobStarted();

            taskPool.CollectTasks(context.CancellationToken);

            if (!context.CancellationToken.IsCancellationRequested)
            {
                await taskPool.ExecuteTasksAsync(DequeueAsync, context.CancellationToken);
            }

            imageConverterContext.OnJobFinished(context);
        }

        private async Task DequeueAsync(QueueItem queueItem)
        {
            ProcessingQueueItem processingQueueItem = processingQueue.AddQueueItem(queueItem);
            try
            {
                string file = queueItem.FullPath;
                FileInfo fileInfo = new FileInfo(file);

                if (fileCleaner!.Clean(queueItem.BaseDirectory, fileInfo))
                {
                    if (!fileInfo.Exists)
                    {
                        imageConverterContext.OnFileDeleted(queueItem, fileInfo.Length);
                    }
                }
                else 
                {
                    if (!IsSkippable(configuration.SkipPostfix!, fileInfo))
                    {
                        long? outputFileSize =
                            await imageConverter!.ConvertImage(processingQueueItem, fileInfo, configuration.Transformers, configuration.OutputFormat!.Value);
                        imageConverterContext.OnImageConverted(processingQueueItem, fileInfo, outputFileSize.Value);
                    }
                    else
                    {
                        imageConverterContext.OnImageIgnored(processingQueueItem);
                        logger.LogWarning($"{file}: skipped by skip postfix settings('{configuration.SkipPostfix!}').");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("There is an error during the image conversion: " + ex.Message);
                imageConverterContext.OnImageConvertFailed(processingQueueItem);
            }
        }   

        private static bool IsSkippable(string? skipPostfix, FileInfo fileInfo)
        {
            return skipPostfix != null && (Path.GetFileNameWithoutExtension(fileInfo.Name).EndsWith(skipPostfix) || (fileInfo.DirectoryName ?? string.Empty).EndsWith(skipPostfix));
        }
    }
}
