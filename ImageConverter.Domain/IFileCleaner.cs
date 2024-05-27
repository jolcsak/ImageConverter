namespace ImageConverter.Domain
{
    public interface IFileCleaner
    {
        bool Clean(string? imageDirectory, FileInfo file);
    }
}