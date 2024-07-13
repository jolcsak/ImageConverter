using ImageConverter.Domain.Queue;

namespace ImageConverter.Domain.Storage
{
    public interface IStorageHandler
    {
        void CancelRunningJobsInStorage();
        IImageConverterSummary ReadImageConverterSummary();
        void Save(IImageConverterSummary? imageSummary, IJobSummary? jobSummary, IQueueItem? updateQueueItem = null, IQueueItem? deleteQueueItem = null);
    }
}