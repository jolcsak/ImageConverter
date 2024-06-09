using ImageConverter.Domain.DbEntities;

namespace ImageConverter.Domain
{
    public interface IStorageHandler
    {
        ImageConverterSummary ReadImageConverterSummary();
        void WriteImageConvertSummary(ImageConverterSummary sumStorage);

        void WriteJobSummary(JobSummary report);
    }
}