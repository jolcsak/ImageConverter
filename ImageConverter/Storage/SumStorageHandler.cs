using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using LiteDB;
using Microsoft.Extensions.Options;

namespace ImageConverter.Storage
{
    public class SumStorageHandler : ISumStorageHandler
    {
        private const string StorageDb = "storage.db";
        private readonly string storageDbPath;

        public SumStorageHandler(IOptions<ImageConverterConfiguration> configurationSettings)
        {
            storageDbPath = Path.Combine(configurationSettings.Value.StoragePath!, StorageDb);
        }

        public async Task WriteSumStorage(SumStorage sumStorage)
        {
            using (var db = new LiteDatabase(storageDbPath))
            {
                var col = db.GetCollection<SumStorage>(nameof(SumStorage));
                col.DeleteAll();
                col.Insert(sumStorage);
            }
        }

        public async Task<SumStorage> ReadSumStorage()
        {
            using (var db = new LiteDatabase(storageDbPath))
            {
                var col = db.GetCollection<SumStorage>(nameof(SumStorage));
                return col.FindAll().FirstOrDefault() ?? new SumStorage(); 
            }
        }
    }
}
