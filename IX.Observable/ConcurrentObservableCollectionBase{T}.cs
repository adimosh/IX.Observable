// <copyright file="ConcurrentObservableCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
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
        private ReaderWriterLockSlim synchronizationLocker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        protected ConcurrentObservableCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext synchronizationContext)
            : base(internalContainer, synchronizationContext)
        {
            this.synchronizationLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
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
        /// A synchronization lock item to be used when trying to synchronize read/write operations between threads.
        /// </summary>
        /// <remarks>
        /// <para>On implemening collections, returns an instance. All read/write operations on the underlying constructs should rely on
        /// the same instance of <see cref="ReaderWriterLockSlim"/> that is returned here to synchronize.</para>
        /// </remarks>
        protected sealed override ReaderWriterLockSlim SynchronizationLock => this.synchronizationLocker;

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
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.synchronizationLocker.Dispose();
                    this.InternalContainer.Clear();
                }

                this.synchronizationLocker = null;
                this.InternalContainer = null;

                this.disposedValue = true;
            }
        }
    }
}