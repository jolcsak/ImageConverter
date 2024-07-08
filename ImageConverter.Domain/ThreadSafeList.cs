
using System.Collections;

namespace ImageConverter.Domain
{
    public class ThreadSafeList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();

        public void Add(T item)
        {
            lock (_lock)
            {
                _list.Add(item);
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    return _list[index];
                }
            }
            set
            {
                lock (_lock)
                {
                    _list[index] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        public void Clear()
        {
            lock(_lock)
            {
                _list.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
           lock (_lock)
           {
                return _list.ToList().GetEnumerator(); 
           }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lock)
            {
                return _list.ToList().GetEnumerator();
            }
        }
    }
}
