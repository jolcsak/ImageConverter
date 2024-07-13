
namespace ImageConverter.Domain.Storage
{
    public interface IImageConverterSummary
    {
        long ConvertedImageCount { get; set; }
        long DeletedFileCount { get; set; }
        long ErrorCount { get; set; }
        long Id { get; set; }
        long IgnoredFileCount { get; set; }
        long InputBytes { get; set; }
        long JobCount { get; set; }
        DateTime? LastFinished { get; set; }
        DateTime LastStarted { get; set; }
        DateTime? NextFire { get; set; }
        long OutputBytes { get; set; }
        string? State { get; set; }
        long SumDeletedFileSize { get; set; }
    }
}