namespace ImageConverter.Domain.Dto
{
    public class SettingsDto
    {
        public ExecutionState ExecutionState { get; set; }
        public DateTime ServerTime { get; set; } 
        public int ThreadCount { get; set; }
        public int QueueLength { get; set; }
        public long MemoryUsage { get; set; }
    }
}
