using ImageConverter.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ImageConverter
{
    public class ApplicationService : BackgroundService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<ApplicationService> logger;
        private readonly IConfigurationHandler configurationHandler;

        public ApplicationService(
            IHostApplicationLifetime appLifetime, 
            ISchedulerFactory schedulerFactory, 
            IConfigurationHandler configurationHandler, 
            ILogger<ApplicationService> logger)
        {
            this.appLifetime = appLifetime;
            this.schedulerFactory = schedulerFactory;
            this.logger = logger;
            this.configurationHandler = configurationHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var configuration = configurationHandler.GetConfiguration();
                logger.LogInformation("Start cron expression {start}", configuration.Starts!);

                CronExpression exp = new CronExpression(configuration.Starts!);
                logger.LogInformation("Next fire time: {nextFireTime}", exp.GetNextValidTimeAfter(DateTime.Now)!.Value.ToLocalTime());

                logger.LogInformation("Using {threadNumber} thread(s) for the conversion process.", configuration.ThreadNumber);

                var jobKey = new JobKey(nameof(ImageConverterJob));
                IJobDetail job = JobBuilder.Create<ImageConverterJob>().WithIdentity(jobKey).Build();
                ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(configuration.Starts!).Build();

                var scheduler = await schedulerFactory.GetScheduler();

                await scheduler.ScheduleJob(job, trigger);

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
