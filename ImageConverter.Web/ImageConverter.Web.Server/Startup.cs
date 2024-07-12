using ImageConverter.Configuration;
using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;
using ImageConverter.Web.Server.Queue;
using ImageConverterStartup = ImageConverter.Startup;

namespace ImageConverter.Web.Server
{
    public static class Startup
    {
        public static void Configure(IHostApplicationBuilder app)
        {
            // configuration
            app.Services.AddSingleton<IConfigurationHandler, ConfigurationHandler>();
            app.Services.Configure<ImageConverterConfiguration>(app.Configuration);
            app.Services.AddOptions();

            app.Services.AddSingleton<IExecutionContext, ExecutionContext>();
            app.Services.AddSingleton<IQueueHandler, QueueHandler>();
            app.Services.AddSingleton<ITaskPool, TaskPool>();
            app.Services.AddSingleton<IProcessingQueue, ProcessingQueue>();
            app.Services.AddSingleton<IImageConverterJobHandler, ImageConverterJobHandler>();

            ImageConverterStartup.Configure(app);
        }
    }
}
