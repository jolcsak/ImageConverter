namespace ImageConverter.Domain.Dto
{
    public class SettingsDto
    {
        public DateTime ServerTime { get; set; } 
        public int ThreadCount { get; set; }
        public int QueueLength { get; set; }
    }
}
