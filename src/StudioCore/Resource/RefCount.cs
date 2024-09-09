using DotNext.Threading;
using System;

namespace StudioCore.Resource
{
    /// <summary>
    /// A refcounted wrapper around a value. When passing a RefCount, the caller must call Ref().
    /// Otherwise, a RefCount must be treated like any other IDisposable. When the ref count reaches 0,
    /// the contained value is automatically disposed if it implements IDisposable.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public class RefCount<T>(T value) : IDisposable
    {
        /// <summary>
        /// The value contained by this RefCount.
        /// </summary>
        public readonly T Value = value;

        internal bool isValid = true;
        internal int Refs = 1;

        /// <summary>
        /// Increments the ref count.
        /// </summary>
        /// <returns></returns>
        public virtual RefCount<T> Ref()
        {
            Refs.IncrementAndGet();
            return this;
        }

        /// <summary>
        /// Determines whether this RefCount is still valid - i.e., true if the contained value has not been
        /// disposed yet. In threaded or async environments, the returned value may immediately be out of date.
        /// </summary>
        /// <returns></returns>
        public bool IsValid => isValid;
        
        /// <summary>
        /// Decrements the ref count, disposing the contained value if the new count is 0.
        /// </summary>
        public virtual void Dispose()
        {
            var r = Refs.DecrementAndGet();
            if (r == 0)
            {
                isValid = false;
                if (Value is IDisposable d) d.Dispose();
            }
        }

        /// <summary>
        /// Decrements the ref count, disposing the contained value if the new count is 0.
        /// </summary>
        public virtual void Dispose(bool force)
        {
            if (force)
            {
                isValid = false;

                if (Value is IDisposable d) 
                    d.Dispose();
            }
            else
            {
                var r = Refs.DecrementAndGet();
                if (r == 0)
                {
                    isValid = false;

                    if (Value is IDisposable d) 
                        d.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// A RefCount that has a parent resource. The parent resource is automatically disposed along with the
    /// contained value when the ref count reaches 0.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="c"></param>
    /// <typeparam name="Parent"></typeparam>
    /// <typeparam name="Child"></typeparam>
    public class ChildResource<Parent, Child>(IDisposable parent, Child c) : RefCount<Child>(c)
    {
        public readonly IDisposable ParentRef = parent;

        public override void Dispose()
        {
            base.Dispose();
            if (Refs == 0) ParentRef.Dispose();
        }
    }
}