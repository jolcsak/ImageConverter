using ImageConverter.Domain.Queue;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Storage.Repositories
{
    public class QueueItemRepository : RepositoryBase, IQueueItemRepository
    {
        private const uint EnqueueBatchSize = 100;

        private readonly object _lock = new object();

        private int enqueueCounter = 0;

        public QueueItemRepository(StorageContext storageContext) : base(storageContext)
        {
        }

        public int Length =>
            DbGet(con => con.Table<QueueItem>().Count(qi => qi.State == (byte)QueueItemState.Queued));

        public void Update(IQueueItem? queueItem)
            => base.Update(queueItem);

        public void Delete(IQueueItem? queueItem)
            => base.Delete(queueItem);

        public void Enqueue(IQueueItem queueItem)
        {
            Db(db =>
            {
                queueItem.State = (byte)QueueItemState.Queued;
                db.Insert(queueItem);
                enqueueCounter++;
                if (enqueueCounter % EnqueueBatchSize == 0)
                {
                    db.Commit();
                }
            });           
        }

        public bool IsNotInQueue(string filePath) 
            => !DbGet(db => db.Table<QueueItem>().Any(q => q.FullPath == filePath));

        public bool TryDequeue(out IQueueItem? queueItem)
        {
            var ret = DbGet(db =>
            {
                lock (_lock)
                {
                    var nextItem = db.Table<QueueItem>().FirstOrDefault(qi => qi.State == (byte)QueueItemState.Queued);
                    QueueItem queueItem = nextItem;
                    if (queueItem != null)
                    {
                        queueItem.State = (byte)QueueItemState.Processing;
                        db.Update(queueItem);
                        return (success: true, queueItem);
                    }
                    return (success: false, queueItem: default(QueueItem));
                }
            });

            queueItem = ret.queueItem;
            return ret.success;
        }

        public void ClearQueue()
        {
            Db(con => con.Table<QueueItem>().Delete(qi => true));
        }
    }
}
