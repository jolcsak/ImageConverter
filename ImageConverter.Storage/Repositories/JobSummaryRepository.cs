using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Storage.Repositories
{
    public class JobSummaryRepository : RepositoryBase, IJobSummaryRepository
    {
        public JobSummaryRepository(StorageContext storageContext) : base(storageContext)
        {
        }

        public void CancelAllRunningJobs()
        {
            Db(db =>
            {
                var runningState = ImageConverterStates.Running.ToString();
                var falseRunningJobs = db.Table<JobSummary>().Where(js => js.State == runningState).ToList();
                if (falseRunningJobs.Any())
                {
                    foreach (var job in falseRunningJobs)
                    {
                        job.State = ImageConverterStates.Cancelled.ToString();
                    }
                    db.UpdateAll(falseRunningJobs);
                }
            });
        }
    }
}
