using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quartz;
using SQLite;
using System.Diagnostics;

namespace ImageConverter.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImageConverterController : ControllerBase
    {
        private const int LogCount = 20;
        private const int JobSummaryCount = 20;

        private readonly string storageLogPath;
        private readonly string sumStoragePath;

        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<ImageConverterController> logger;      
        private readonly ImageConverterJobRegistry imageConverterJobRegistry;
        private readonly ImageConverterConfiguration configuration;
        private readonly ITaskPool taskPool;
        private readonly IProcessingQueue processingQueue;
        private readonly IExecutionContext executionContext;

        public ImageConverterController(
            IOptions<ImageConverterConfiguration> configurationSettings, 
            ILogger<ImageConverterController> logger,
            ISchedulerFactory schedulerFactory,
            ImageConverterJobRegistry imageConverterJobRegistry,
            ITaskPool taskPool,
            IProcessingQueue processingQueue,
            IExecutionContext executionContext)
        {
            configuration = configurationSettings.Value;

            storageLogPath = Path.Combine(configuration.StoragePath!, Constants.LogsDb);
            sumStoragePath = Path.Combine(configuration.StoragePath!, Constants.StorageDb);
            this.schedulerFactory = schedulerFactory;
            this.logger = logger;
            this.imageConverterJobRegistry = imageConverterJobRegistry;
            this.taskPool = taskPool;
            this.processingQueue = processingQueue;
            this.executionContext = executionContext;
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
                await scheduler.TriggerJob(imageConverterJobRegistry.JobKey!);
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<string> StopJob()
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (await IsImageConverterJobRunningAsync(scheduler))
            {
                await scheduler.Interrupt(imageConverterJobRegistry.JobKey!);
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<string> ClearQueue()
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (await IsImageConverterJobRunningAsync(scheduler))
            {
                await scheduler.Interrupt(imageConverterJobRegistry.JobKey!);
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
            using (var db = new SQLiteConnection(storageLogPath))
            {
                return db.Table<Logs>()
                    .OrderByDescending(l => l.Id)
                    .Take(LogCount)
                    .Select(l => new LogMessage(l.Timestamp, l.RenderedMessage, l.Level)).ToArray();
            }
        }

        [HttpGet]
        public IEnumerable<JobSummary> GetJobSummaries()
        {
            using (var db = new SQLiteConnection(sumStoragePath))
            {
                return db.Table<JobSummary>()
                    .OrderByDescending(l => l.Id)
                    .Take(JobSummaryCount)
                    .ToArray();
            }
        }

        [HttpGet]
        public ImageConverterSummary GetImageConverterSummary()
        {
            using (var db = new SQLiteConnection(sumStoragePath))
            {
                return db.Table<ImageConverterSummary>().FirstOrDefault() ?? new ImageConverterSummary();
            }
        }

        [HttpGet]
        public JobSummary GetJobSummary()
        {
            using (var db = new SQLiteConnection(sumStoragePath))
            {
                return db.Table<JobSummary>().LastOrDefault() ?? new JobSummary();
            }
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

            return imageConverterJobRegistry.JobKey != null && 
                   executingJobs.Any(ej => ej.JobDetail.Key.Equals(imageConverterJobRegistry.JobKey));
        }
    }
}
