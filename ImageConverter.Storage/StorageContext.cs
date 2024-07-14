using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;
using ImageConverter.Storage.Repositories;
using Microsoft.Extensions.Options;
using SQLite;

namespace ImageConverter.Storage
{
    public class StorageContext : StorageContextBase, IStorageContext
    {
        private string storageDbPath;
        private readonly IOptions<ImageConverterConfiguration> configurationSettings;

        public IQueueItemRepository QueueItemRepository { get; private set; }
        public IJobSummaryRepository JobSummaryRepository { get; private set; }
        public IImageConverterSummaryRepository ImageConverterSummaryRepository { get; private set; }

        protected override string StoragePath => storageDbPath;

        public StorageContext(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            this.configurationSettings = configurationSettings;
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.StorageDb);

            CreateNotExistingDbObjects();

            QueueItemRepository = new QueueItemRepository(this);
            JobSummaryRepository = new JobSummaryRepository(this);
            ImageConverterSummaryRepository = new ImageConverterSummaryRepository(this);
        }

        private StorageContext(IOptions<ImageConverterConfiguration> configurationSettings, SQLiteConnection connection) : this(configurationSettings)
        {
            Connection = connection;
        }

        private void CreateNotExistingDbObjects()
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                db.CreateTable<ImageConverterSummary>();
                db.CreateTable<JobSummary>();
                db.CreateTable<QueueItem>();
            }
        }

        public IStorageContext CreateTransaction()
        {
            return new StorageContext(configurationSettings, CreateConnection());
        }
    }
}
