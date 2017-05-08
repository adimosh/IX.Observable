// <copyright file="WriteOnlySynchronizationLocker.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved.
// </copyright>

using System;
using System.Threading;

namespace IX.Observable.SynchronizationLockers
{
    /// <summary>
    /// A write-only synchronization locker.
    /// </summary>
    /// <seealso cref="IX.Observable.SynchronizationLockers.SynchronizationLocker" />
    public class WriteOnlySynchronizationLocker : SynchronizationLocker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteOnlySynchronizationLocker"/> class.
        /// </summary>
        /// <param name="locker">The locker.</param>
        /// <exception cref="TimeoutException">The lock could not be acquired in time.</exception>
        public WriteOnlySynchronizationLocker(ReaderWriterLockSlim locker)
            : base(locker)
        {
            if (!locker?.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout) ?? false)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Releases the currently-held lock.
        /// </summary>
        public override void Dispose() => this.Locker?.ExitWriteLock();
    }
}