using SQLite;

namespace ImageConverter.Storage
{
    public abstract class StorageContextBase
    {
        internal SQLiteConnection? Connection { get; set; }

        protected abstract string StoragePath { get; }

        public SQLiteConnection CreateConnection()
        {
            return Connection ?? new SQLiteConnection(StoragePath);
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
            }
        }
    }
}