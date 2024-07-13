using ImageConverter.Domain.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageConverter.Storage
{
    public static class Startup
    {
        public static void Configure(IHostApplicationBuilder app)
        {
            app.Services.AddSingleton<IStorageContext, StorageContext>();
        }
    }
}
