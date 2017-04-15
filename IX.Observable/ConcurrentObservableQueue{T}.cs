// <copyright file="ConcurrentObservableQueue{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(QueueDebugView<>))]
    public class ConcurrentObservableQueue<T> : ConcurrentObservableCollectionBase<T>, IQueue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        public ConcurrentObservableQueue()
            : base(new QueueCollectionAdapter<T>(new Queue<T>()), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ConcurrentObservableQueue(IEnumerable<T> collection)
            : base(new QueueCollectionAdapter<T>(new Queue<T>(collection)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ConcurrentObservableQueue(int capacity)
            : base(new QueueCollectionAdapter<T>(new Queue<T>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableQueue(SynchronizationContext context)
            : base(new QueueCollectionAdapter<T>(new Queue<T>()), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy from.</param>
        public ConcurrentObservableQueue(SynchronizationContext context, IEnumerable<T> collection)
            : base(new QueueCollectionAdapter<T>(new Queue<T>(collection)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ConcurrentObservableQueue(SynchronizationContext context, int capacity)
            : base(new QueueCollectionAdapter<T>(new Queue<T>(capacity)), context)
        {
        }

        /// <summary>
        /// Dequeues and removes an item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public T Dequeue()
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                T item;
                try
                {
                    item = ((QueueCollectionAdapter<T>)this.InternalContainer).queue.Dequeue();
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.RaisePropertyChanged(nameof(this.Count));
                        this.RaisePropertyChanged(Constants.ItemsName);
                        this.RaiseCollectionChangedRemove(state, 0);
                    }, item);

                return item;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Enqueues an item into the queue.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((QueueCollectionAdapter<T>)this.InternalContainer).queue.Enqueue(item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.RaisePropertyChanged(nameof(this.Count));
                        this.RaisePropertyChanged(Constants.ItemsName);
                        this.RaiseCollectionChangedAdd(state.item, state.index);
                    }, new { index = this.Count - 1, item });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Peeks at the topmost item in the queue without dequeueing it.
        /// </summary>
        /// <returns>The topmost item in the queue.</returns>
        public virtual T Peek()
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((QueueCollectionAdapter<T>)this.InternalContainer).queue.Peek();
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public virtual T[] ToArray()
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((QueueCollectionAdapter<T>)this.InternalContainer).queue.ToArray();
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ConcurrentObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess()
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((QueueCollectionAdapter<T>)this.InternalContainer).queue.TrimExcess();
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                return;
            }

            throw new TimeoutException();
        }
    }
}