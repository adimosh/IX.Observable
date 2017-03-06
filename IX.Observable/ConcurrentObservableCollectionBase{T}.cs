// <copyright file="ConcurrentObservableCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for collections that are observable and concurrent.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{T}" />
    public abstract class ConcurrentObservableCollectionBase<T> : ObservableCollectionBase<T>, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        protected ConcurrentObservableCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext synchronizationContext)
            : base(internalContainer, synchronizationContext)
        {
            this.Locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConcurrentObservableCollectionBase{T}"/> class.
        /// </summary>
        ~ConcurrentObservableCollectionBase()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override int Count
        {
            get
            {
                if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return this.InternalContainer.Count;
                    }
                    finally
                    {
                        this.Locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the locker object used to synchronize reading and writing operations.
        /// </summary>
        /// <value>
        /// The locker object.
        /// </value>
        protected ReaderWriterLockSlim Locker { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds an item to the <see cref="ConcurrentObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ConcurrentObservableCollectionBase{T}" />.</param>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override void Add(T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                int newIndex;
                try
                {
                    newIndex = this.InternalContainer.Add(item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                {
                    if (state.index == -1)
                    {
                        this.OnCollectionChanged();
                    }
                    else
                    {
                        this.OnCollectionChangedAdd(state.item, state.index);
                    }

                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                }, new { index = newIndex, item });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ConcurrentObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ConcurrentObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="ConcurrentObservableCollectionBase{T}" />; otherwise, <c>false</c>. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ConcurrentObservableCollectionBase{T}" />.
        /// </returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override bool Remove(T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                int oldIndex;
                try
                {
                    oldIndex = this.InternalContainer.Remove(item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                if (oldIndex >= 0)
                {
                    this.AsyncPost(
                        (state) =>
                    {
                        this.OnCollectionChangedRemove(state.item, state.index);
                        this.OnPropertyChanged(nameof(this.Count));
                        this.ContentsMayHaveChanged();
                    }, new { index = oldIndex, item });
                    return true;
                }
                else if (oldIndex < -1)
                {
                    this.AsyncPost(() =>
                    {
                        this.OnCollectionChanged();
                        this.OnPropertyChanged(nameof(this.Count));
                        this.ContentsMayHaveChanged();
                    });
                    return true;
                }
                else
                {
                    return false;
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Returns a locking enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <remarks>
        /// <para>The enumerator, by default, locks the collection in place and ensures that any attempts to modify from the same thread will result in an exception,
        /// whereas from a different thread will result in the other thread patiently waiting for its turn to write.</para>
        /// <para>This implementation focuses on the normal use of enumerators, which is to dispose of their IEnumerator at the end of their enumeration cycle.</para>
        /// <para>If the enumerator is never disposed of, it will never release the read lock, thus making the other threads time out.</para>
        /// <para>Please make sure that you dispose the enumerator object at all times in order to avoid deadlocking and timeouts.</para>
        /// </remarks>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override IEnumerator<T> GetEnumerator()
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    using (IEnumerator<T> enumerator = this.InternalContainer.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            yield return enumerator.Current;
                        }
                    }

                    yield break;
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Removes all items from the <see cref="ConcurrentObservableCollectionBase{T}" />.
        /// </summary>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override void Clear()
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalContainer.Clear();
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentObservableCollectionBase{T}" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ConcurrentObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ConcurrentObservableCollectionBase{T}" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override bool Contains(T item)
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return this.InternalContainer.Contains(item);
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Copies the elements of the <see cref="ConcurrentObservableCollectionBase{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ConcurrentObservableCollectionBase{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalContainer.CopyTo(array, arrayIndex);
                    return;
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.Locker.Dispose();
                    this.InternalContainer.Clear();
                }

                this.Locker = null;
                this.InternalContainer = null;

                this.disposedValue = true;
            }
        }
    }
}