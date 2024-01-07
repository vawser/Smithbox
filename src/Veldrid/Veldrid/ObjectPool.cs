using System.Collections.Generic;
using System.Threading;

namespace Veldrid
{
    internal class ObjectPool<T> where T : new()
    {
        private int _maxSize;
        private SpinLock _lock;
        private Stack<T> _pool;

        public ObjectPool(int size)
        {
            _maxSize = size;
            _lock = new SpinLock(false);
            _pool = new Stack<T>(_maxSize);
        }

        public T Get()
        {
            bool taken = false;
            _lock.Enter(ref taken);
            try
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
            }
            finally
            {
                if (taken)
                {
                    _lock.Exit(false);
                }
            }

            return new T();
        }

        public void Recycle(T obj)
        {
            bool taken = false;
            _lock.Enter(ref taken);
            try
            {
                if (_pool.Count < _maxSize)
                {
                    _pool.Push(obj);
                }
            }
            finally
            {
                if (taken)
                {
                    _lock.Exit(false);
                }
            }
        }
    }
}