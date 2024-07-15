

namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IJobSummaryRepository
    {
        IJobSummary GetLastJobSummary();
        IJobSummary New(DateTime jobStarted, string? state);
        void Upsert(IJobSummary? jobSummary);
        IEnumerable<IJobSummary> GetJobSummaries();
        void CancelAllRunningJobs();
    }
}
