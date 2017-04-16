// <copyright file="ReadOnlySynchronizationLocker.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Threading;

namespace IX.Observable.SynchronizationLockers
{
    /// <summary>
    /// A read-only synchronization locker.
    /// </summary>
    /// <seealso cref="IX.Observable.SynchronizationLockers.SynchronizationLocker" />
    public class ReadOnlySynchronizationLocker : SynchronizationLocker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlySynchronizationLocker"/> class.
        /// </summary>
        /// <param name="locker">The locker.</param>
        /// <exception cref="TimeoutException">The lock could not be acquired in time.</exception>
        public ReadOnlySynchronizationLocker(ReaderWriterLockSlim locker)
            : base(locker)
        {
            if (!locker?.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout) ?? false)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Releases the currently-held lock.
        /// </summary>
        public override void Dispose() => this.Locker?.ExitReadLock();
    }
}