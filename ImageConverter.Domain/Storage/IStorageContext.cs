
namespace ImageConverter.Domain.Storage
{
    public interface IStorageContext : IDisposable
    {
        public IQueueItemRepository QueueItemRepository { get; }

        public IStorageContext CreateTransaction();
    }
}
