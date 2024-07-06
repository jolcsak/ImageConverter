using ImageConverter.Domain.DbEntities;
using SQLite;

namespace ImageConverter.Domain
{
    public interface IStorageHandler
    {
        void CancelRunningJobsInStorage();
        SQLiteConnection GetConnection();
        ImageConverterSummary ReadImageConverterSummary();
        void Save(ImageConverterSummary? imageSummary, JobSummary? jobSummary, QueueItem? updateQueueItem = null, QueueItem? deleteQueueItem = null);
    }
}