// <copyright file="ReadWriteSynchronizationLocker.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Threading;

namespace IX.Observable.SynchronizationLockers
{
    /// <summary>
    /// A read and write synchronization locker.
    /// </summary>
    /// <seealso cref="IX.Observable.SynchronizationLockers.SynchronizationLocker" />
    public class ReadWriteSynchronizationLocker : SynchronizationLocker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteSynchronizationLocker"/> class.
        /// </summary>
        /// <param name="locker">The locker.</param>
        /// <exception cref="TimeoutException">The lock could not be acquired in time.</exception>
        public ReadWriteSynchronizationLocker(ReaderWriterLockSlim locker)
            : base(locker)
        {
            if (!locker?.TryEnterUpgradeableReadLock(Constants.ConcurrentLockAcquisitionTimeout) ?? false)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Upgrades the lock to a write lock.
        /// </summary>
        /// <exception cref="TimeoutException">The lock could not be acquired in time.</exception>
        public void Upgrade()
        {
            if (!this.Locker?.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout) ?? false)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Releases the currently-held lock.
        /// </summary>
        public override void Dispose()
        {
            if (this.Locker != null)
            {
                if (this.Locker.IsWriteLockHeld)
                {
                    this.Locker.ExitWriteLock();
                }

                if (this.Locker.IsUpgradeableReadLockHeld)
                {
                    this.Locker.ExitUpgradeableReadLock();
                }
            }
        }
    }
}