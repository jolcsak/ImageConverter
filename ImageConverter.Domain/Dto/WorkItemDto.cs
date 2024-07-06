namespace ImageConverter.Domain.Dto
{
    public enum WorkItemDtoState
    {
        Waiting,
        Processing,
        Converted,
        Failed,
        Cancelled
    }

    public class WorkItemDto
    {
        public string FilePath { get; set; }
        public WorkItemDtoState State { get; set; }
        public DateTime? ProcessingStarted { get; set; }
        public DateTime? ProcessingCompleted { get; set; }

    }
}
