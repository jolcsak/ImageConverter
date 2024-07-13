namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IImageConverterSummaryRepository
    {
        IImageConverterSummary GetImageConverterSummary();
        void Upsert(IImageConverterSummary? imageConverterSummary);
    }
}