using ImageConverter.ConversionRules.In;
using ImageConverter.ConversionRules.Out;
using ImageConverter.Domain.ImageConverter;
using ImageConverter.Domain.ImageConverter.ConversionRules;
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
        }
    }
}
