using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Storage.Repositories
{
    public class ImageConverterSummaryRepository : RepositoryBase, IImageConverterSummaryRepository
    {
        public ImageConverterSummaryRepository(StorageContext storageContext) : base(storageContext)
        {
        }

        public IImageConverterSummary GetImageConverterSummary()
            => DbGet(db => db.Table<ImageConverterSummary>().FirstOrDefault() ?? new ImageConverterSummary());

    }
}
