namespace ImageConverter.Domain.Storage.Repositories
{
    public interface IJobSummaryRepository
    {
        void CancelAllRunningJobs();
    }
}
