
using ImageConverter.Domain.Storage.Repositories;

namespace ImageConverter.Domain.Storage
{
    public interface IStorageContext : IDisposable
    {
        public IQueueItemRepository QueueItemRepository { get; }
        IJobSummaryRepository JobSummaryRepository { get; }
        IImageConverterSummaryRepository ImageConverterSummaryRepository { get; }

        public IStorageContext CreateTransaction();
    }
}
