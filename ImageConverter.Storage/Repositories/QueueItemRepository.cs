using ImageConverter.Domain.Queue;
using ImageConverter.Domain.Storage.Repositories;
using ImageConverter.Storage.Entities;

namespace ImageConverter.Storage.Repositories
{
    public class QueueItemRepository : RepositoryBase, IQueueItemRepository
    {
        private const uint EnqueueBatchSize = 100;

        private static readonly object _dbLock = new object();

        private static int? length = null;

        private int enqueueCounter = 0;

        public QueueItemRepository(StorageContext storageContext) : base(storageContext)
        {
        }

        public int Length
        {
            get {
                lock (_dbLock)
                {
                    if (length == null)
                    {
                        length = DbGet(con => con.Table<QueueItem>().Count(qi => qi.State == (byte)QueueItemState.Queued));
                    }
                    return length.Value;
                }
            }
        }

        public void Update(IQueueItem? queueItem)
        {
            lock (_dbLock)
            {
                base.Update(queueItem);
            }
        }

        public void Delete(IQueueItem? queueItem)
        {
            lock (_dbLock)
            {
                length -= base.Delete(queueItem);
            }
        }

        public void Enqueue(string baseDirectory, string filePath)
        {
            lock (_dbLock)
            {
                Db(db =>
                {
                    IQueueItem queueItem = new QueueItem
                    {
                        BaseDirectory = baseDirectory,
                        FullPath = filePath,
                        State = (byte)QueueItemState.Queued
                    };
                    db.Insert(queueItem);
                    enqueueCounter++;
                    if (enqueueCounter % EnqueueBatchSize == 0)
                    {
                        db.Commit();
                    }

                    length++;
                });
            }
        }

        public bool IsNotInQueue(string filePath)
        {
            lock (_dbLock)
            {
                return !DbGet(db => db.Table<QueueItem>().Any(q => q.FullPath == filePath));
            }
        }

        public bool TryDequeue(out IQueueItem? queueItem)
        {
            lock (_dbLock)
            {

                var ret = DbGet(db =>
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
                });

                queueItem = ret.queueItem;
                return ret.success;
            }
        }

        public void ClearQueue()
        {
            lock (_dbLock)
            {
                Db(con => con.Table<QueueItem>().Delete(qi => true));
                length = 0;
            }            
        }
    }
}
