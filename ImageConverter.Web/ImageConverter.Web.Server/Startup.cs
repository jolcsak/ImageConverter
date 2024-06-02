using ImageConverter.Domain.Dto;
using ImageConverterStartup = ImageConverter.Startup;

namespace ImageConverter.Web.Server
{
    public static class Startup
    {
        public static void Configure(IHostApplicationBuilder app)
        {
            app.Services.AddSingleton<ImageConverterJobRegistry>();
            ImageConverterStartup.Configure(app);
        }
    }
}
