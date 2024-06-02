using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quartz;
using SQLite;

namespace ImageConverter.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImageConverterController : ControllerBase
    {
        private const int LogCount = 15;

        private readonly string storageLogPath;
        private readonly string sumStoragePath;

        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<ImageConverterController> logger;
        private readonly ImageConverterJobRegistry imageConverterJobRegistry;

        public ImageConverterController(
            IOptions<ImageConverterConfiguration> configurationSettings, 
            ILogger<ImageConverterController> logger,
            ISchedulerFactory schedulerFactory,
            ImageConverterJobRegistry imageConverterJobRegistry)
        {
            storageLogPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.LogsDb);
            sumStoragePath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.StorageDb);
            this.schedulerFactory = schedulerFactory;
            this.logger = logger;
            this.imageConverterJobRegistry = imageConverterJobRegistry;
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
        public SumStorage GetSummaries()
        {
            using (var db = new SQLiteConnection(sumStoragePath))
            {
                return db.Table<SumStorage>().FirstOrDefault() ?? new SumStorage();
            }
        }

        private async Task<bool> IsImageConverterJobRunningAsync(IScheduler scheduler)
        {
            IReadOnlyCollection<IJobExecutionContext> executingJobs = await scheduler.GetCurrentlyExecutingJobs();

            return imageConverterJobRegistry.JobKey != null && 
                   executingJobs.Any(ej => ej.JobDetail.Key.Equals(imageConverterJobRegistry.JobKey));
        }
    }
}
