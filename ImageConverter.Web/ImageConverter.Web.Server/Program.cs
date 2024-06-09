using ImageConverter.Domain.Dto;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using ImageConverter.Web.Server;
using ImageConverter.Domain;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var configuration = builder.Configuration.Get<ImageConverterConfiguration>();
string logDbPath = Path.Combine(configuration?.StoragePath ?? string.Empty, Constants.LogsDb);

var logger = new LoggerConfiguration()
.MinimumLevel.Debug()
.WriteTo.Console(theme: AnsiConsoleTheme.None)
.WriteTo.SQLite(sqliteDbPath: logDbPath, batchSize: 1, retentionPeriod: new TimeSpan(7, 0, 0, 0))
.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddHostedService<ApplicationService>();

Startup.Configure(builder);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
