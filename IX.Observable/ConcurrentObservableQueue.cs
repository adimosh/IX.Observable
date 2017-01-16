// <copyright file="ConcurrentObservableQueue.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(QueueDebugView<>))]
    public class ConcurrentObservableQueue<T> : ObservableQueue<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        public ConcurrentObservableQueue()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ConcurrentObservableQueue(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ConcurrentObservableQueue(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableQueue(SynchronizationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy from.</param>
        public ConcurrentObservableQueue(SynchronizationContext context, IEnumerable<T> collection)
            : base(context, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ConcurrentObservableQueue(SynchronizationContext context, int capacity)
            : base(context, capacity)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        ~ConcurrentObservableQueue()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        public override int Count
        {
            get
            {
                if (this.locker.TryEnterReadLock(this.timeout))
                {
                    try
                    {
                        return base.Count;
                    }
                    finally
                    {
                        this.locker.ExitReadLock();
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines whether the queue contains a specific item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the queue contains a specific item, <c>false</c> otherwise.</returns>
        public override bool Contains(T item)
        {
            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.Contains(item);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Copies the contents of the queue to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy the items into.</param>
        /// <param name="arrayIndex">The index at which to start in the array.</param>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    base.CopyTo(array, arrayIndex);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets an enumerator for the concurrent observable queue.
        /// </summary>
        /// <returns>The queue enumerator.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            T[] items;

            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    items = this.InternalContainer.ToArray();
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }

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
            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.Peek();
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public override T[] ToArray()
        {
            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.ToArray();
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ConcurrentObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public override void TrimExcess()
        {
            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    base.TrimExcess();
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Clears the queue of all its objects (internal overridable procedure).
        /// </summary>
        protected override void ClearInternal()
        {
            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    base.ClearInternal();
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="disposing"><c>true</c> for normal disposal, where normal operation should dispose sub-objects,
        /// <c>false</c> for a GC disposal without the normal pattern.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.locker.Dispose();
                }

                this.InternalContainer.Clear();
                this.InternalContainer = null;

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Dequeues and removes an item from the queue (internal overridable procedure).
        /// </summary>
        /// <returns>The dequeued item.</returns>
        protected override T DequeueInternal()
        {
            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    return base.DequeueInternal();
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Enqueues an item into the queue (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        protected override void EnqueueInternal(T item)
        {
            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    base.EnqueueInternal(item);
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}