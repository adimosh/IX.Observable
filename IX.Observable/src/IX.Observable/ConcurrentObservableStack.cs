using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IX.Observable
{
    /// <summary>
    /// A thread-safe stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    public class ConcurrentObservableStack<T> : ObservableStack<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        public ConcurrentObservableStack()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableStack(SynchronizationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(SynchronizationContext context, int capacity)
            : base(context, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(SynchronizationContext context, IEnumerable<T> collection)
            : base(context, collection)
        {
        }

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
        // ~ConcurrentObservableStack() {
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

        /// <summary>
        /// Checks whether or not a certain item is in the stack.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns><c>true</c> if the item was found, <c>false</c> otherwise.</returns>
        public override bool Contains(T item)
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return internalContainer.Contains(item);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// The number of elements in the observable stack.
        /// </summary>
        public override int Count
        {
            get
            {
                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return internalContainer.Count;
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the thread-safe enumerator for this collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            T[] array;

            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    array = internalContainer.ToArray();

                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            else
                throw new TimeoutException();

            foreach (T item in array)
            {
                yield return item;
            }

            yield break;
        }

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public override T[] ToArray()
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return internalContainer.ToArray();
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
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public override T Peek()
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return internalContainer.Peek();
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
        /// Pops the topmost element from the stack, removing it (internal overridable procedure).
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        protected override T PopInternal()
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    return internalContainer.Pop();
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
        /// Pushes an element to the top of the stack (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to push.</param>
        protected override void PushInternal(T item)
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    internalContainer.Push(item);
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
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public override void TrimExcess()
        {
            if (locker.TryEnterUpgradeableReadLock(timeout))
            {
                try
                {
                    internalContainer.TrimExcess();
                }
                finally
                {
                    locker.ExitUpgradeableReadLock();
                }
            }
            else
                throw new TimeoutException();
        }
    }
}