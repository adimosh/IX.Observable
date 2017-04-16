// <copyright file="ConcurrentObservableMasterSlaveCollection{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Diagnostics;
using System.Threading;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{TItem}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ConcurrentObservableMasterSlaveCollection<T> : ObservableMasterSlaveCollection<T>
    {
        private ReaderWriterLockSlim locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableMasterSlaveCollection{T}"/> class.
        /// </summary>
        public ConcurrentObservableMasterSlaveCollection()
            : base()
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableMasterSlaveCollection{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        public ConcurrentObservableMasterSlaveCollection(SynchronizationContext context)
            : base(context)
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Gets a synchronization lock item to be used when trying to synchronize read/write operations between threads.
        /// </summary>
        protected override ReaderWriterLockSlim SynchronizationLock => this.locker;

        /// <summary>
        /// Disposes of this instance and performs necessary cleanup.
        /// </summary>
        /// <param name="managedDispose">Indicates whether or not the call came from <see cref="System.IDisposable"/> or from the destructor.</param>
        protected override void Dispose(bool managedDispose)
        {
            if (managedDispose)
            {
                this.locker.Dispose();
            }

            base.Dispose(managedDispose);

            this.locker = null;
        }
    }
}