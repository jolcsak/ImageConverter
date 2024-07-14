using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;
using ImageConverter.Domain.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quartz;
using System.Diagnostics;

namespace ImageConverter.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImageConverterController : ControllerBase
    {
        private readonly string storageLogPath;
        private readonly string sumStoragePath;

        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<ImageConverterController> logger;      
        private readonly ImageConverterConfiguration configuration;
        private readonly ITaskPool taskPool;
        private readonly IStorageContext storageContext;
        private readonly ILogStorageContext logStorageContext;
        private readonly IProcessingQueue processingQueue;
        private readonly IExecutionContext executionContext;

        public ImageConverterController(
            IOptions<ImageConverterConfiguration> configurationSettings, 
            ILogger<ImageConverterController> logger,
            ISchedulerFactory schedulerFactory,
            ITaskPool taskPool,
            IProcessingQueue processingQueue,
            IExecutionContext executionContext,
            IStorageContext storageContext,
            ILogStorageContext logStorageContext)
        {
            configuration = configurationSettings.Value;

            storageLogPath = Path.Combine(configuration.StoragePath!, Constants.LogsDb);
            sumStoragePath = Path.Combine(configuration.StoragePath!, Constants.StorageDb);
            this.schedulerFactory = schedulerFactory;
            this.logger = logger;
            this.taskPool = taskPool;
            this.processingQueue = processingQueue;
            this.executionContext = executionContext;
            this.storageContext = storageContext;
            this.logStorageContext = logStorageContext;
        }

        [HttpGet]
        public async Task<bool> IsImageConverterJobRunning()
        {
            var scheduler = await schedulerFactory.GetScheduler();
            return await IsImageConverterJobRunningAsync(scheduler);
        }

        [HttpGet]
        public async Task<string> StartJob()
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (!await IsImageConverterJobRunningAsync(scheduler))
            {
                await scheduler.TriggerJob(executionContext.JobKey!);
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<string> StopJob()
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (await IsImageConverterJobRunningAsync(scheduler))
            {
                await scheduler.Interrupt(executionContext.JobKey!);
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<string> ClearQueue()
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (await IsImageConverterJobRunningAsync(scheduler))
            {
                await scheduler.Interrupt(executionContext.JobKey!);
            }

            taskPool.ClearQueue();
            processingQueue.ClearQueue();

            return string.Empty;
        }

        [HttpGet]
        public SettingsDto GetSettings()
        {
            return new SettingsDto
            {
                ServerTime = DateTime.UtcNow,
                ThreadCount = 
                    configuration.ThreadNumber.HasValue? 
                        configuration.ThreadNumber.Value : Environment.ProcessorCount,
                QueueLength = taskPool.QueueLength,
                MemoryUsage = Process.GetCurrentProcess().WorkingSet64,
                ExecutionState = executionContext.ExecutionState
            };
        }

        [HttpGet]
        public IEnumerable<LogMessage> GetLogs()
        {
            return logStorageContext.LogsRepository.GetLastLogMessages();
        }

        [HttpGet]
        public IEnumerable<IJobSummary> GetJobSummaries()
        {
            return storageContext.JobSummaryRepository.GetJobSummaries();
        }

        [HttpGet]
        public IImageConverterSummary GetImageConverterSummary()
        {
            return storageContext.ImageConverterSummaryRepository.GetImageConverterSummary();
        }

        [HttpGet]
        public IJobSummary GetJobSummary()
        {
            return storageContext.JobSummaryRepository.GetLastJobSummary();
        }

        [HttpGet]
        public IEnumerable<ProcessingQueueItem> GetProcessingQueue()
        {
            return processingQueue.GetLastQueueItems();
        }

        [HttpGet]
        public IEnumerable<string> GetProcessingPaths()
        {
            return processingQueue.GetLastProcessingPaths();
        }

        private async Task<bool> IsImageConverterJobRunningAsync(IScheduler scheduler)
        {
            IReadOnlyCollection<IJobExecutionContext> executingJobs = await scheduler.GetCurrentlyExecutingJobs();

            return executionContext.JobKey != null && 
                   executingJobs.Any(ej => ej.JobDetail.Key.Equals(executionContext.JobKey));
        }
    }
}
