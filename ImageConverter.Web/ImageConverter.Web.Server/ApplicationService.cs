using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;
using Quartz;

namespace ImageConverter.Web.Server
{
    public class ApplicationService : BackgroundService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<ApplicationService> logger;
        private readonly IConfigurationHandler configurationHandler;
        private readonly IExecutionContext executionContext;
        private readonly IImageConverterJobHandler imageConverterContext;

        public ApplicationService(
            IHostApplicationLifetime appLifetime,
            ISchedulerFactory schedulerFactory,
            IConfigurationHandler configurationHandler,
            IImageConverterJobHandler imageConverterContext,
            ILogger<ApplicationService> logger,
            IExecutionContext executionContext)
        {
            this.appLifetime = appLifetime;
            this.schedulerFactory = schedulerFactory;
            this.logger = logger;
            this.configurationHandler = configurationHandler;          
            this.executionContext = executionContext;
            this.imageConverterContext = imageConverterContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var configuration = configurationHandler.GetConfiguration();
                logger.LogInformation("Start cron expression {start}", configuration.Starts!);

                CronExpression exp = new CronExpression(configuration.Starts!);
                logger.LogInformation("Using {threadNumber} thread(s) for the conversion process.", configuration.ThreadNumber);

                var jobKey = new JobKey(nameof(ImageConverterJob));
                IJobDetail job = JobBuilder.Create<ImageConverterJob>().WithIdentity(jobKey).Build();
                ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(configuration.Starts!).Build();

                executionContext.JobKey = jobKey;
                executionContext.Trigger = trigger;

                var scheduler = await schedulerFactory.GetScheduler();

                await scheduler.ScheduleJob(job, trigger);

                UpdateSummaries(configuration, trigger);

                if (configuration.RunAtStart == true)
                {
                    logger.LogInformation("Configuraton.RunAtStart={runAtStart} -> ImageConverter job starting now...", configuration.RunAtStart);
                    await scheduler.TriggerJob(jobKey);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during registering background jobs. Exiting...");
                appLifetime.StopApplication();
            }
            finally
            {
                appLifetime.ApplicationStopping.Register(StopQuartzServices);
            }
        }

        private void UpdateSummaries(ImageConverterConfiguration configuration, ITrigger trigger)
        {
            imageConverterContext.Sum.State = ImageConverterStates.NotStarted.ToString();

            DateTimeOffset? nextFireTimeUtc = trigger.GetNextFireTimeUtc();
            if (nextFireTimeUtc != null)
            {
                DateTime nextFireTime = nextFireTimeUtc.Value.LocalDateTime;
                imageConverterContext.Sum.NextFire = nextFireTime;
                logger.LogInformation("Next fire time: {nextFireTime}", nextFireTime);
            }
            else
            {
                logger.LogWarning("Next fire time ({startExpr}) not set!", configuration.Starts!);
            }

            imageConverterContext.Save(saveJobSummary: false);
        }

        private void StopQuartzServices()
        {
            try
            {
                schedulerFactory.GetScheduler().Result.Shutdown();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during background jobs shutdown.");
            }
        }
    }
}
