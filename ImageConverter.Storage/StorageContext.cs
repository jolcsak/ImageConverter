using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using ImageConverter.Storage.Repositories;
using Microsoft.Extensions.Options;
using SQLite;

namespace ImageConverter.Storage
{
    public class StorageContext : IStorageContext
    {
        private string storageDbPath;
        private readonly IOptions<ImageConverterConfiguration> configurationSettings;
        internal SQLiteConnection? Connection { get; set; }

        public IQueueItemRepository QueueItemRepository { get; private set; }

        public StorageContext(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            this.configurationSettings = configurationSettings;
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.StorageDb);
            QueueItemRepository = new QueueItemRepository(this);
        }

        private StorageContext(IOptions<ImageConverterConfiguration> configurationSettings, SQLiteConnection connection) : this(configurationSettings)
        {
            Connection = connection;
        }

        public SQLiteConnection CreateConnection()
        {
            return Connection ?? new SQLiteConnection(storageDbPath);
        }

        public IStorageContext CreateTransaction()
        {
            return new StorageContext(configurationSettings, CreateConnection());
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
            }
        }
    }
}
