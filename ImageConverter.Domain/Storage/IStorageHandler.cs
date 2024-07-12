using SQLite;

namespace ImageConverter.Domain.Storage
{
    public interface IStorageHandler
    {
        void CancelRunningJobsInStorage();
        void ClearQueue();
        SQLiteConnection GetConnection();
        ImageConverterSummary ReadImageConverterSummary();
        void Save(ImageConverterSummary? imageSummary, JobSummary? jobSummary, QueueItem? updateQueueItem = null, QueueItem? deleteQueueItem = null);
    }
}