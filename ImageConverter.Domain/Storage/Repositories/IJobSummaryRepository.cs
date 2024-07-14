
namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IJobSummaryRepository
    {
        IJobSummary GetLastJobSummary();
        void Upsert(IJobSummary? jobSummary);
        IEnumerable<IJobSummary> GetJobSummaries();
        void CancelAllRunningJobs();
    }
}
