// <copyright file="ConcurrentObservableListBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for lists that are observable and concurrent.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ConcurrentObservableCollectionBase{T}" />
    /// <seealso cref="IList{T}" />
    /// <seealso cref="IReadOnlyList{T}" />
    public class ConcurrentObservableListBase<T> : ConcurrentObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableListBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        public ConcurrentObservableListBase(ListAdapter<T> internalContainer, SynchronizationContext synchronizationContext)
            : base(internalContainer, synchronizationContext)
        {
        }

        /// <summary>
        /// Gets the internal list container.
        /// </summary>
        /// <value>
        /// The internal list container.
        /// </value>
        protected ListAdapter<T> InternalListContainer => (ListAdapter<T>)this.InternalContainer;

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public T this[int index]
        {
            get
            {
                if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return this.InternalListContainer[index];
                    }
                    finally
                    {
                        this.Locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }

            set
            {
                if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        this.InternalListContainer[index] = value;
                    }
                    finally
                    {
                        this.Locker.ExitWriteLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Determines the index of a specific item, if any.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index of the item, or <c>-1</c> if not found.</returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public int IndexOf(T item)
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return this.InternalListContainer.IndexOf(item);
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public void Insert(int index, T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalListContainer.Insert(index, item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to remove an item from.</param>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public void RemoveAt(int index)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalListContainer.RemoveAt(index);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }
            }

            throw new TimeoutException();
        }
    }
}