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

        public void Upsert(IImageConverterSummary? imageConverterSummary)
            => Db(db =>
            {
                if (imageConverterSummary != null)
                {
                    if (db.Table<ImageConverterSummary>().FirstOrDefault() == null)
                    {
                        db.Insert(imageConverterSummary);
                    }
                    else
                    {
                        db.Update(imageConverterSummary);
                    }
                }
            });

    }
}
