using ImageConverter.Domain.Storage;

namespace ImageConverter.Domain.Dto
{
    public enum ProcessingQueueItemState
    {
        Compressing,
        Recompressing,
        Compressed,
        Ignored,
        Deleted,
        Failed
    }

    public class ProcessingQueueItem
    {
        public string Path { get; set; }
        public ProcessingQueueItemState State { get; set; }
        public long InputFileSize { get; set; }
        public long OutputFileSize { get; set; }
        public int Quality { get; set; }

        public ProcessingQueueItem(string path, ProcessingQueueItemState state)
        {
            State = state;
            this.Path = path;
        }
    }
}
