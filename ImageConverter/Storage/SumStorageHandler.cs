using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using Microsoft.Extensions.Options;
using SQLite;

namespace ImageConverter.Storage
{
    public class SumStorageHandler : ISumStorageHandler
    {
        private readonly string storageDbPath;

        public SumStorageHandler(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, Constants.StorageDb);
            using (var db = new SQLiteConnection(storageDbPath))
            {
                db.CreateTable<SumStorage>();
            }
        }

        public void WriteSumStorage(SumStorage sumStorage)
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                var sum = db.Table<SumStorage>().FirstOrDefault();
                if (sum == null)
                {
                    db.Insert(sumStorage);
                }
                else
                {
                    sum.ProcessedBytes = sumStorage.ProcessedBytes;
                    sum.SumSavedBytes = sumStorage.SumSavedBytes;
                    sum.ConvertedImageCount = sumStorage.ConvertedImageCount;
                    sum.DeletedFileCount = sumStorage.DeletedFileCount;
                    sum.ErrorCount = sumStorage.ErrorCount;
                    sum.SumDeleteFileSize = sumStorage.SumDeleteFileSize;
                    sum.LastStarted = sumStorage.LastStarted;
                    sum.LastFinished = sumStorage.LastFinished;
                    sum.NextFire = sumStorage.NextFire;
                    sum.State = sumStorage.State;
                    db.Update(sum);
                }
            }
        }

        public SumStorage ReadSumStorage()
        {
            using (var db = new SQLiteConnection(storageDbPath))
            {
                return db.Table<SumStorage>().FirstOrDefault() ?? new SumStorage();
            }
        }
    }
}
