using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Storage.Repositories.Log;
using ImageConverter.Storage.Repositories.Log;
using Microsoft.Extensions.Options;
using SQLite;

namespace ImageConverter.Storage
{
    public class LogStorageContext : StorageContextBase, ILogStorageContext
    {
        private string storageDbPath;
        private readonly IOptions<ImageConverterConfiguration> configurationSettings;
        
        protected override string StoragePath => storageDbPath;

        public ILogsRepository LogsRepository { get; set; }

        public LogStorageContext(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            this.configurationSettings = configurationSettings;
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.LogsDb);

            LogsRepository = new LogsRepository(this);
        }

        private LogStorageContext(IOptions<ImageConverterConfiguration> configurationSettings, SQLiteConnection connection) : this(configurationSettings)
        {
            Connection = connection;
        }

        public ILogStorageContext CreateTransaction()
        {
            return new LogStorageContext(configurationSettings, CreateConnection());
        }
    }
}
