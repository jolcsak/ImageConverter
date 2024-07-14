using SQLite;

namespace ImageConverter.Storage.Repositories
{
    public class RepositoryBase
    {
        protected readonly StorageContextBase storageContext;

        protected RepositoryBase(StorageContextBase storageContext)
        { 
            this.storageContext = storageContext;
        }

        protected void Db(Action<SQLiteConnection> a)
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

        protected T DbGet<T>(Func<SQLiteConnection, T> f)
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

        public void Delete<T>(T entity)
        {
            Db(db =>
            {
                if (entity != null)
                {
                    db.Delete(entity);
                }
            });
        }
    }
}