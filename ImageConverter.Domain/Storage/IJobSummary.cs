
namespace ImageConverter.Domain.Storage
{
    public interface IJobSummary
    {
        long ConvertedImageCount { get; set; }
        long DeletedFileCount { get; set; }
        long ErrorCount { get; set; }
        long Id { get; set; }
        long IgnoredFileCount { get; set; }
        long InputBytes { get; set; }
        DateTime? JobFinished { get; set; }
        DateTime JobStarted { get; set; }
        long OutputBytes { get; set; }
        string? State { get; set; }
        long SumDeletedFileSize { get; set; }
    }
}