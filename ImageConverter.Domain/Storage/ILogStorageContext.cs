using ImageConverter.Domain.Storage.Repositories.Log;

namespace ImageConverter.Domain.Storage
{
    public interface ILogStorageContext
    {
        ILogsRepository LogsRepository { get; set; }

        ILogStorageContext CreateTransaction();
    }
}