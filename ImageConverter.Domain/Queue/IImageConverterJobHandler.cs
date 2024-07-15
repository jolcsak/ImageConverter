using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Storage;
using Quartz;

namespace ImageConverter.Domain.Queue
{
    public interface IImageConverterJobHandler
    {
        IImageConverterSummary? AllSummary { get; }
        IJobSummary? JobSummary { get; }

        void OnFileDeleted(IQueueItem queueItem, long fileSize);
        void OnImageConverted(IQueueItem queueItem, ProcessingQueueItem processingQueueItem, FileInfo inputFileInfo, long? outputFileSize);
        void OnImageConvertFailed(IQueueItem queueItem, ProcessingQueueItem processingQueueItem);
        void OnImageIgnored(IQueueItem queueItem, ProcessingQueueItem processingQueueItem);
        void OnJobFinished(IJobExecutionContext context);
        void OnJobStarted();
        void Save(IQueueItem? updateQueueItem = null, IQueueItem? deleteQueueItem = null, bool saveJobSummary = true);
    }
}