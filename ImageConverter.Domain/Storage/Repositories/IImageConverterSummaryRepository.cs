namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IImageConverterSummaryRepository
    {
        IImageConverterSummary GetImageConverterSummary();
        void Update(IImageConverterSummary? imageConverterSummary);
        void Upsert(IImageConverterSummary? imageConverterSummary);
    }
}