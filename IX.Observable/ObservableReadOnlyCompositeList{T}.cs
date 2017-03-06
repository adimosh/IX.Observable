// <copyright file="ObservableReadOnlyCompositeList{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// An observable, composite, thread-safe and readonly list made of multiple lists of the same rank.
    /// </summary>
    /// <typeparam name="T">The type of the list item.</typeparam>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="IX.Observable.ObservableReadOnlyCollectionBase{T}" />
    public class ObservableReadOnlyCompositeList<T> : ObservableReadOnlyCollectionBase<T>, IDisposable
    {
        private bool disposedValue;
        private ReaderWriterLockSlim locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCompositeList{T}"/> class.
        /// </summary>
        public ObservableReadOnlyCompositeList()
            : base(new MultiListListAdapter<T>(), null)
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCompositeList{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        public ObservableReadOnlyCompositeList(SynchronizationContext context)
            : base(new MultiListListAdapter<T>(), context)
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ObservableReadOnlyCompositeList{T}"/> class.
        /// </summary>
        ~ObservableReadOnlyCompositeList()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the required lock.</exception>
        public override int Count
        {
            get
            {
                if (this.locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return this.InternalContainer.Count;
                    }
                    finally
                    {
                        this.locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

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
        /// Determines whether the <see cref="T:IX.Observable.ObservableCollectionBase`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:IX.Observable.ObservableCollectionBase`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:IX.Observable.ObservableCollectionBase`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the required lock.</exception>
        public override bool Contains(T item)
        {
            if (this.locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return this.InternalContainer.Contains(item);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:IX.Observable.ObservableCollectionBase`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:IX.Observable.ObservableCollectionBase`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the required lock.</exception>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            if (this.locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalContainer.CopyTo(array, arrayIndex);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.TimeoutException">There was a timeout acquiring the required lock.</exception>
        public override IEnumerator<T> GetEnumerator()
        {
            if (this.locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
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
                    this.locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Sets a list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void SetList<TList>(TList list)
                    where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            if (this.locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((MultiListListAdapter<T>)this.InternalContainer).SetList(list);
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.OnPropertyChanged(Constants.ItemsName);
                });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Removes a list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void RemoveList<TList>(TList list)
                    where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            if (this.locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((MultiListListAdapter<T>)this.InternalContainer).RemoveList(list);
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.OnPropertyChanged(Constants.ItemsName);
                });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.locker.Dispose();
                    this.InternalContainer.Clear();
                }

                this.locker = null;
                this.InternalContainer = null;

                this.disposedValue = true;
            }
        }
    }
}