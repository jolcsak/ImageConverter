using ImageConverter.Domain;
using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Options;
using SQLite;
using ImageConverter.Storage.Entities;
using ImageConverter.Domain.Queue;

namespace ImageConverter.Web.Server.Storage
{
    public class StorageHandler : IStorageHandler
    {
        private readonly string storageDbPath;

        public StorageHandler(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.StorageDb);
            using (var db = new SQLiteConnection(storageDbPath))
            {
                db.CreateTable<ImageConverterSummary>();
                db.CreateTable<JobSummary>();
                db.CreateTable<QueueItem>();
            }
        }

        public SQLiteConnection GetConnection() => new SQLiteConnection(storageDbPath);

        public void Save(IImageConverterSummary? imageSummary, IJobSummary? jobSummary, IQueueItem? updateQueueItem = null, IQueueItem? deleteQueueItem = null)
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                if (imageSummary != null)
                {
                    if (db.Table<ImageConverterSummary>().FirstOrDefault() == null)
                    {
                        db.Insert(imageSummary);
                    }
                    else
                    {
                        db.Update(imageSummary);
                    }
                }
                if (jobSummary != null)
                {
                    if (jobSummary.Id == 0)
                    {
                        db.Insert(jobSummary);
                    }
                    else
                    {
                        db.Update(jobSummary);
                    }
                }

                if (updateQueueItem != null)
                {
                    db.Update(updateQueueItem);
                }

                if (deleteQueueItem != null)
                {
                    db.Delete(deleteQueueItem);
                }
            }
        }
    }
}
