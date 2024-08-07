﻿using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Storage.Repositories
{
    public class JobSummaryRepository : RepositoryBase, IJobSummaryRepository
    {
        private const int JobSummaryCount = 20;

        public JobSummaryRepository(StorageContext storageContext) : base(storageContext)
        {
        }

        public IJobSummary New(DateTime jobStarted, string? state)
        {
            return DbGet(db =>
                {
                    var jobSummary = new JobSummary { JobStarted = jobStarted, State = state };
                    db.Insert(jobSummary);
                    return jobSummary;
                }
            );
        }

        public void Upsert(IJobSummary? jobSummary)
        {
            Db(db =>
            {
                if (jobSummary != null)
                {
                    if (jobSummary.Id == 0)
                    {
                        db.Insert(jobSummary);
                    }
                    else
                    {
                        db.Update(jobSummary);
                    }
                }
            });
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

        public IJobSummary GetLastJobSummary()
        {
            return DbGet(db => db.Table<JobSummary>().LastOrDefault() ?? new JobSummary());
        }

        public IEnumerable<IJobSummary> GetJobSummaries()
        {
            return DbGet(db =>
            {
                return db.Table<JobSummary>()
                                    .OrderByDescending(l => l.Id)
                                    .Take(JobSummaryCount)
                                    .ToArray();
            });
        }
    }
}
