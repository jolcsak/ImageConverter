using SQLite;

namespace ImageConverter.Storage.Repositories
{
    public class RepositoryBase
    {
        private static readonly object _dbLock = new object();

        protected readonly StorageContextBase storageContext;

        protected RepositoryBase(StorageContextBase storageContext)
        { 
            this.storageContext = storageContext;
        }

        protected void Db(Action<SQLiteConnection> a)
        {
            lock (_dbLock)
            {
                if (storageContext.Connection == null)
                {
                    using (SQLiteConnection connection = storageContext.CreateConnection())
                    {
                        a(connection);
                    }
                }
                else
                {
                    a(storageContext.Connection);
                }
            }
        }

        protected T DbGet<T>(Func<SQLiteConnection, T> f)
        {
            lock (_dbLock)
            {
                if (storageContext.Connection == null)
                {
                    using (SQLiteConnection connection = storageContext.CreateConnection())
                    {
                        return f(connection);
                    }
                }
                else
                {
                    return f(storageContext.Connection);
                }
            }
        }

        public void Update<T>(T entity)
        {
            Db(db =>
            {
                if (entity != null)
                {
                    db.Update(entity);
                }
            });
        }

        public int Delete<T>(T entity)
        {
            return DbGet(db =>
            {
                return entity != null ? db.Delete(entity) : 0;
            });
        }
    }
}