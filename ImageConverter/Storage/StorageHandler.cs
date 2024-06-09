using ImageConverter.Domain;
using ImageConverter.Domain.DbEntities;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Options;
using SQLite;

namespace ImageConverter.Storage
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
            }
        }

        public void WriteImageConvertSummary(ImageConverterSummary sumStorage)
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                if (db.Table<ImageConverterSummary>().FirstOrDefault() == null)
                {
                    db.Insert(sumStorage);
                }
                else
                {
                    db.Update(sumStorage);
                }
            }
        }

        public ImageConverterSummary ReadImageConverterSummary()
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                return db.Table<ImageConverterSummary>().FirstOrDefault() ?? new ImageConverterSummary();
            }
        }

        public void WriteJobSummary(JobSummary jobSummary)
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                if (jobSummary.Id == 0) { 
                    db.Insert(jobSummary);
                }
                else
                {
                    db.Update(jobSummary);
                }
            }
        }
    }
}
