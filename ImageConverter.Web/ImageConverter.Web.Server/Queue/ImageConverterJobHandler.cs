using ImageConverter.Domain.Queue;
using ImageConverter.Domain.Storage;
using ImageConverter.Storage.Entities;
using NeoSmart.PrettySize;
using Quartz;
using System.Diagnostics;

namespace ImageConverter.Domain.Dto
{
    public class ImageConverterJobHandler : IImageConverterJobHandler
    {
        private readonly object _lock = new();

        public IImageConverterSummary Sum { get; private set; } = new ImageConverterSummary();
        public IJobSummary JobSummary { get; private set; } = new JobSummary();

        private readonly IStorageContext storageContext;
        private readonly ILogger<ImageConverterJobHandler> logger;
        private readonly IExecutionContext executionContext;

        private DateTime jobStarted;
        private Stopwatch sw = new Stopwatch();

        public ImageConverterJobHandler(
            IStorageContext storageContext,
            IExecutionContext executionContext,
            ILogger<ImageConverterJobHandler> logger)
        {
            this.storageContext = storageContext;
            this.executionContext = executionContext;
            this.logger = logger;
        }

        public void OnJobStarted()
        {
            jobStarted = DateTime.Now;

            using (IStorageContext conn = storageContext.CreateTransaction())
            {
                conn.JobSummaryRepository.CancelAllRunningJobs();
                Sum = conn.ImageConverterSummaryRepository.GetImageConverterSummary();
            }
            Sum.State = ImageConverterStates.Running.ToString();
            Sum.LastStarted = jobStarted;
            Sum.JobCount++;

            JobSummary = new JobSummary { JobStarted = jobStarted, State = Sum.State };

            Save();

            sw = Stopwatch.StartNew();
        }

        public void Save(IQueueItem? updateQueueItem = null, IQueueItem? deleteQueueItem = null, bool saveJobSummary = true)
        {
            using (IStorageContext conn = storageContext.CreateTransaction())
            {
                conn.ImageConverterSummaryRepository.Upsert(Sum);
                conn.JobSummaryRepository.Upsert(saveJobSummary ? JobSummary : null);
                conn.QueueItemRepository.Update(updateQueueItem);
                conn.QueueItemRepository.Delete(deleteQueueItem);
            }
        }

        public void OnFileDeleted(IQueueItem queueItem, long fileSize)
        {
            lock (_lock)
            {
                JobSummary.DeletedFileCount++;
                JobSummary.SumDeletedFileSize += fileSize;
                Sum.DeletedFileCount++;
                Save(deleteQueueItem: queueItem);
            }
        }

        public void OnImageConverted(IQueueItem queueItem, ProcessingQueueItem processingQueueItem, FileInfo inputFileInfo, long? outputFileSize)
        {
            if (outputFileSize == null)
            {
                return;
            }

            lock (_lock)
            {
                Sum.InputBytes += inputFileInfo.Length;
                Sum.OutputBytes += outputFileSize.Value;
                Sum.ConvertedImageCount++;

                JobSummary.InputBytes += inputFileInfo.Length;
                JobSummary.OutputBytes += outputFileSize.Value;
                JobSummary.ConvertedImageCount++;

                inputFileInfo.Delete();
                processingQueueItem.State = ProcessingQueueItemState.Compressed;
                queueItem.State = (byte)QueueItemState.Processed;
                Save(deleteQueueItem: queueItem);
            }
        }

        public void OnImageIgnored(IQueueItem queueItem, ProcessingQueueItem processingQueueItem)
        {
            lock (_lock)
            {
                JobSummary.IgnoredFileCount++;
                Sum.IgnoredFileCount++;
                processingQueueItem.State = ProcessingQueueItemState.Ignored;
                queueItem.State = (byte)QueueItemState.Ignored;
                Save(updateQueueItem: queueItem);
            }
        }

        public void OnImageConvertFailed(IQueueItem queueItem, ProcessingQueueItem processingQueueItem)
        {
            lock (_lock)
            {
                JobSummary.ErrorCount++;
                Sum.ErrorCount++;
                processingQueueItem.State = ProcessingQueueItemState.Failed;
                queueItem.State = (byte)QueueItemState.Error;
                Save(updateQueueItem: queueItem);
            }
        }

        public void OnJobFinished(IJobExecutionContext context)
        {
            LogReport();

            DateTimeOffset? nextFireTimeUtc = executionContext.Trigger!.GetNextFireTimeUtc();

            if (nextFireTimeUtc != null)
            {
                DateTime nextFireTime = nextFireTimeUtc.Value.LocalDateTime;
                logger.LogInformation("Next fire time {nextFireTime}", nextFireTime);
                Sum.NextFire = nextFireTime;
            }
            else
            {
                Sum.NextFire = null;
            }

            Sum.State = context.CancellationToken.IsCancellationRequested ?
                ImageConverterStates.Cancelled.ToString() :
                ImageConverterStates.Finished.ToString();

            Sum.LastFinished = DateTime.Now;
            JobSummary.JobFinished = Sum.LastFinished;
            JobSummary.State = Sum.State;
            Save();
        }

        private void LogStatistics()
        {
            IImageConverterSummary sumStorage = Sum;

            var prettyProcessedBytes = PrettySize.Bytes(sumStorage.InputBytes);
            var prettySumSavedBytes = PrettySize.Bytes(sumStorage.OutputBytes);
            var prettySumDeletedFileSize = PrettySize.Bytes(sumStorage.SumDeletedFileSize);

            logger.LogInformation("******************************************************************************************************************");
            logger.LogInformation("Totally converted images: {convertedImageCount}, processed: {processedBytes}, saved: {sumSavedBytes}, cleaned #: {deletedFileCount}, cleaned: {sumDeletedFileSize}, ignored #: {ignoredCount}",
                sumStorage.ConvertedImageCount, prettyProcessedBytes.Format(UnitBase.Base10), prettySumSavedBytes.Format(UnitBase.Base10),
                sumStorage.DeletedFileCount, prettySumDeletedFileSize.Format(UnitBase.Base10), Sum.IgnoredFileCount);
            logger.LogInformation("******************************************************************************************************************");
        }

        private void LogReport()
        {
            sw.Stop();

            var prettyInputSumSize = PrettySize.Bytes(JobSummary.InputBytes);
            var prettyOutputSumSize = PrettySize.Bytes(JobSummary.OutputBytes);
            var prettySavedSumSize = PrettySize.Bytes(JobSummary.InputBytes - JobSummary.OutputBytes);
            var prettySumDeletedFileSize = PrettySize.Bytes(JobSummary.SumDeletedFileSize);

            logger.LogInformation("Conversion done: {convertedCount} files converted, {inputSumSize} -> {outputSumSize}, saved: {savedSumSize}, cleaned #: {deletedCount}, cleaned size: {sumDeletedFileSize}, ignored #: {ignoredCount}, took {totalSeconds} seconds.",
                JobSummary.ConvertedImageCount,
                prettyInputSumSize.Format(UnitBase.Base10),
                prettyOutputSumSize.Format(UnitBase.Base10),
                prettySavedSumSize.Format(UnitBase.Base10),
                JobSummary.DeletedFileCount,
                prettySumDeletedFileSize.Format(UnitBase.Base10),
                JobSummary.IgnoredFileCount,
                sw.Elapsed.TotalSeconds);

            LogStatistics();
        }
    }
}
