using ImageConverter;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

internal class Program
{
    private const string LogsDb = "logs.db";

    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddJsonFile("/config/appsettings.json", optional: true, reloadOnChange: true);

        Startup.Configure(builder);

        builder.Services.AddQuartz();
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        var configuration = builder.Configuration.Get<ImageConverterConfiguration>();
        string logDbPath = Path.Combine(configuration?.StoragePath ?? string.Empty, LogsDb);

        var logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(theme: AnsiConsoleTheme.None)
        .WriteTo.LiteDB($"Filename={logDbPath};connection=shared", batchPostingLimit: 1)
        .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        builder.Services.AddHostedService<ApplicationService>();

        IHost host = builder.Build();

        await host.RunAsync();
    }
}