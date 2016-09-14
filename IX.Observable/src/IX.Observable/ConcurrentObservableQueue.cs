using System;
using System.Collections.Generic;
using System.Threading;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    public class ConcurrentObservableQueue<T> : ObservableQueue<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        public ConcurrentObservableQueue()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ConcurrentObservableQueue(IEnumerable<T> collection)
            : base(collection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ConcurrentObservableQueue(int capacity)
            : base(capacity)
        { }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="disposing"><c>true</c> for normal disposal, where normal operation should dispose sub-objects,
        /// <c>false</c> for a GC disposal without the normal pattern.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    locker.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                internalContainer.Clear();
                internalContainer = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ConcurrentObservableQueue() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Clears the queue of all its objects (internal overridable procedure).
        /// </summary>
        protected override void ClearInternal()
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    base.ClearInternal();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Determines whether the queue contains a specific item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the queue contains a specific item, <c>false</c> otherwise.</returns>
        public override bool Contains(T item)
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.Contains(item);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Copies the contents of the queue to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy the items into.</param>
        /// <param name="arrayIndex">The index at which to start in the array.</param>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    base.CopyTo(array, arrayIndex);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        public override int Count
        {
            get
            {
                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return base.Count;
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }
                else
                    throw new TimeoutException();
            }
        }

        /// <summary>
        /// Dequeues and removes an item from the queue (internal overridable procedure).
        /// </summary>
        /// <returns>The dequeued item.</returns>
        protected override T DequeueInternal()
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    return base.DequeueInternal();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Enqueues an item into the queue (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        protected override void EnqueueInternal(T item)
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    base.EnqueueInternal(item);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Gets an enumerator for the concurrent observable queue.
        /// </summary>
        /// <returns>The queue enumerator.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            T[] items;

            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    items = internalContainer.ToArray();
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();

            foreach (var item in items)
            {
                yield return item;
            }

            yield break;
        }

        /// <summary>
        /// Peeks at the topmost item in the queue without dequeueing it.
        /// </summary>
        /// <returns>The topmost item in the queue.</returns>
        public override T Peek()
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.Peek();
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public override T[] ToArray()
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.ToArray();
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ConcurrentObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public override void TrimExcess()
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    base.TrimExcess();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
                throw new TimeoutException();
        }
    }
}