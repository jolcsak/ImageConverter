using ImageConverter.Configuration;
using ImageConverter.ConversionRules.In;
using ImageConverter.ConversionRules.Out;
using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Storage;
using ImageConverter.Transformers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageConverter
{
    public static class Startup
    {
        public static void Configure(IHostApplicationBuilder app)
        {
            app.Services.AddTransient<IConversionInRule, PngInRule>();

            app.Services.AddTransient<IConversionOutRule, WebpOutRule>();

            app.Services.AddTransient<IImageConverter, ImageConverter>();

            app.Services.AddTransient<IImageTransformer, ImageResizeTransformer>();

            app.Services.AddTransient<IFileCleaner, FileCleaner>();

            app.Services.AddSingleton<ISumStorageHandler, SumStorageHandler>();

            // configuration

            app.Services.Configure<ImageConverterConfiguration>(app.Configuration);

            app.Services.AddSingleton<IConfigurationHandler, ConfigurationHandler>();

            app.Services.AddOptions();
        }
    }
}
