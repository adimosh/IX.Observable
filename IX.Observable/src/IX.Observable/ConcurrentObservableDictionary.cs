using System;
using System.Collections.Generic;
using System.Threading;

namespace IX.Observable
{
    /// <summary>
    /// A thread-safe dictionary that broadcasts its changes.
    /// </summary>
    /// <typeparam name="TKey">The data key type.</typeparam>
    /// <typeparam name="TValue">The data value type.</typeparam>
    public class ConcurrentObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ConcurrentObservableDictionary()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ConcurrentObservableDictionary(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base(equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(capacity, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ConcurrentObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }
        #endregion

        /// <summary>
        /// Attempts to fetch a value for a specific key in a thread-safe manner, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully fetched, <c>false</c> otherwise.</returns>
        public override bool TryGetValue(TKey key, out TValue value)
        {
            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.TryGetValue(key, out value);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets a thread-safe enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (!locker.TryEnterReadLock(timeout))
                yield break;

            KeyValuePair<TKey, TValue>[] array;

            try
            {
                array = new KeyValuePair<TKey, TValue>[internalContainer.Count];
                ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).CopyTo(array, 0);
            }
            finally
            {
                locker.ExitReadLock();
            }

            foreach (var kvp in array)
                yield return kvp;

            yield break;
        }

        /// <summary>
        /// Clears all items from the dictionary (internal overridable procedure).
        /// </summary>
        protected override void ClearInternal()
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    base.ClearInternal();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <param name="value">The value that was removed, if any.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected override bool RemoveInternal(TKey key, out TValue value)
        {
            if (locker.TryEnterUpgradeableReadLock(timeout))
            {
                try
                {
                    if (!internalContainer.TryGetValue(key, out value))
                        return false;

                    if (locker.TryEnterWriteLock(timeout))
                    {
                        try
                        {
                            return internalContainer.Remove(key);
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    if (locker?.IsUpgradeableReadLockHeld ?? false)
                        locker.ExitUpgradeableReadLock();
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Attempts to remove a key/value pair (internal overridable procedure).
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected override bool RemoveInternal(KeyValuePair<TKey, TValue> item)
        {
            if (locker.TryEnterWriteLock(timeout))
            {
                try
                {
                    return ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).Remove(item);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            return false;
        }
    }
}