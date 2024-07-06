using ImageConverter.Domain.Dto;
using ImageConverter.Domain.QueueHandler;
using ImageConverterStartup = ImageConverter.Startup;

namespace ImageConverter.Web.Server
{
    public static class Startup
    {
        public static void Configure(IHostApplicationBuilder app)
        {
            app.Services.AddSingleton<IQueueHandler, QueueHandler.QueueHandler>();

            app.Services.AddSingleton<ImageConverterJobRegistry>();
            ImageConverterStartup.Configure(app);
        }
    }
}
