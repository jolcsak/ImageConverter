namespace ImageConverter.Domain.ImageConverter
{
    public interface IFileCleaner
    {
        bool Clean(string? imageDirectory, FileInfo file);
    }
}