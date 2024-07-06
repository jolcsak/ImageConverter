using ImageConverter.Domain.DbEntities;
using Microsoft.Extensions.Logging;
using NeoSmart.PrettySize;
using Quartz;
using System.Diagnostics;

namespace ImageConverter.Domain.Dto
{
    public class ImageConverterContext
    {
        public ImageConverterSummary Sum { get; private set; } = new ImageConverterSummary();
        public JobSummary JobSummary { get; private set; } = new JobSummary();

        private readonly IStorageHandler storageHandler;
        private ILogger<ImageConverterContext> logger;
        private readonly ImageConverterJobRegistry imageConverterJobRegistry;

        private readonly object lockObject = new();

        private DateTime jobStarted;
        private Stopwatch sw = new Stopwatch();

        public ImageConverterContext(
            IStorageHandler storageHandler,
            ImageConverterJobRegistry imageConverterJobRegistry,
            ILogger<ImageConverterContext> logger)
        {
            this.storageHandler = storageHandler;
            this.imageConverterJobRegistry = imageConverterJobRegistry;
            this.logger = logger;
        }

        public void OnJobStarted()
        {
            jobStarted = DateTime.Now;

            storageHandler.CancelRunningJobsInStorage();

            Sum = storageHandler.ReadImageConverterSummary();
            Sum.State = ImageConverterStates.Running.ToString();
            Sum.LastStarted = jobStarted;
            Sum.JobCount++;

            JobSummary = new JobSummary { JobStarted = jobStarted, State = Sum.State };

            Save();

            sw = Stopwatch.StartNew();
        }

        public void Save(QueueItem? updateQueueItem = null, QueueItem? deleteQueueItem = null, bool saveJobSummary = true)
        {
            storageHandler.Save(Sum, saveJobSummary ? JobSummary : null, updateQueueItem, deleteQueueItem);
        }

        public void OnFileDeleted(QueueItem queueItem, long fileSize)
        {
            lock (lockObject)
            {
                JobSummary.DeletedFileCount++;
                JobSummary.SumDeletedFileSize += fileSize;
                Sum.DeletedFileCount++;
                Save(deleteQueueItem: queueItem);
            }
        }

        public void OnImageConverted(QueueItem queueItem, FileInfo inputFileInfo, long? outputFileSize)
        {
            if (outputFileSize == null)
            {
                return;
            }

            lock (lockObject)
            {

                Sum.InputBytes += inputFileInfo.Length;
                Sum.OutputBytes += outputFileSize.Value;
                Sum.ConvertedImageCount++;

                JobSummary.InputBytes += inputFileInfo.Length;
                JobSummary.OutputBytes += outputFileSize.Value;
                JobSummary.ConvertedImageCount++;

                inputFileInfo.Delete();

                queueItem.State = (byte)QueueItemState.Processed;
                Save(deleteQueueItem: queueItem);
            }
        }

        public void OnImageIgnored(QueueItem queueItem)
        {
            lock (lockObject)
            {
                JobSummary.IgnoredFileCount++;
                Sum.IgnoredFileCount++;
                queueItem.State = (byte)QueueItemState.Ignored;
                Save(updateQueueItem:queueItem);
            }
        }

        public void OnImageConvertFailed(QueueItem queueItem)
        {
            lock (lockObject)
            {
                JobSummary.ErrorCount++;
                Sum.ErrorCount++;
                queueItem.State = (byte)QueueItemState.Error;
                Save(updateQueueItem: queueItem);
            }
        }

        public void OnJobFinished(IJobExecutionContext context)
        {
            LogReport();

            DateTimeOffset? nextFireTimeUtc = imageConverterJobRegistry.Trigger!.GetNextFireTimeUtc();

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
            ImageConverterSummary sumStorage = Sum;

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
