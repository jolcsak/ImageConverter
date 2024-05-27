using ImageConverter.Domain.Dto;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageConverter.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private const string LogsDb = "logs.db";

        private readonly string storageLogPath;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            IOptions<ImageConverterConfiguration> configurationSettings, 
            ILogger<WeatherForecastController> logger)
        {
            storageLogPath = Path.Combine(configurationSettings.Value.StoragePath!, LogsDb);
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<LogEvent> Get()
        {
            using (var db = new LiteDatabase($"Filename={storageLogPath};connection=shared"))
            {
                var col = db.GetCollection("log");
                foreach (var document in col.FindAll())
                {
                    yield return new LogEvent()
                    {
                        Date = document["_t"].AsDateTime,
                        Message = document["_m"].AsString,
                        Level = document["_l"].AsString
                    };
                }
            }

        }
    }
}
