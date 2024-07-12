using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using Quartz;

namespace ImageConverter.Domain.Queue
{
    public interface IImageConverterJobHandler
    {
        JobSummary JobSummary { get; }
        ImageConverterSummary Sum { get; }

        void OnFileDeleted(QueueItem queueItem, long fileSize);
        void OnImageConverted(ProcessingQueueItem processingQueueItem, FileInfo inputFileInfo, long? outputFileSize);
        void OnImageConvertFailed(ProcessingQueueItem processingQueueItem);
        void OnImageIgnored(ProcessingQueueItem processingQueueItem);
        void OnJobFinished(IJobExecutionContext context);
        void OnJobStarted();
        void Save(QueueItem? updateQueueItem = null, QueueItem? deleteQueueItem = null, bool saveJobSummary = true);
    }
}