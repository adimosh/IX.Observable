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
    public class ConcurrentObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDisposable
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="disposing"><c>true</c> for normal disposal, where normal operation should dispose sub-objects,
        /// <c>false</c> for a GC disposal without the normal pattern.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    locker.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                internalContainer.Clear();
                internalContainer = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ConcurrentObservableDictionary() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
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
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey,TValue>));

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

            throw new TimeoutException();
        }

        /// <summary>
        /// Gets a thread-safe enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

            if (!locker.TryEnterReadLock(timeout))
                throw new TimeoutException();

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
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

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
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <param name="value">The value that was removed, if any.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected override bool RemoveInternal(TKey key, out TValue value)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

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

            throw new TimeoutException();
        }

        /// <summary>
        /// Attempts to remove a key/value pair (internal overridable procedure).
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected override bool RemoveInternal(KeyValuePair<TKey, TValue> item)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

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

            throw new TimeoutException();
        }

        /// <summary>
        /// Copies the elements of the dictionary into an array, at the specified index.
        /// </summary>
        /// <param name="array">The array to copy elements into.</param>
        /// <param name="arrayIndex">The index at which to start copying items.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).CopyTo(array, arrayIndex);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
                throw new TimeoutException();
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public override bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.Contains(item);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public override bool ContainsKey(TKey key)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

            if (locker.TryEnterReadLock(timeout))
            {
                try
                {
                    return base.ContainsKey(key);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Adds an item to the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected override void AddInternal(TKey key, TValue value)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

            if (locker.TryEnterUpgradeableReadLock(timeout))
            {
                try
                {
                    if (internalContainer.ContainsKey(key))
                        throw new ArgumentException(Resources.DictionaryItemAlreadyExists, nameof(key));

                    if (locker.TryEnterWriteLock(timeout))
                    {
                        try
                        {
                            internalContainer.Add(key, value);
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                    else
                        throw new TimeoutException();
                }
                finally
                {
                    if (locker?.IsUpgradeableReadLockHeld ?? false)
                        locker.ExitUpgradeableReadLock();
                }
            }
            else
                throw new TimeoutException();

        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        public override ICollection<TKey> Keys
        {
            get
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return base.Keys;
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public override ICollection<TValue> Values
        {
            get
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return base.Values;
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs in the dictionary.
        /// </summary>
        public override int Count
        {
            get
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return base.Count;
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets or sets the value associated with a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the specified key.</returns>
        public override TValue this[TKey key]
        {
            get
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

                if (locker.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return base[key];
                    }
                    finally
                    {
                        locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
            set
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));

                if (locker.TryEnterUpgradeableReadLock(timeout))
                {
                    try
                    {
                        TValue val;
                        if (internalContainer.TryGetValue(key, out val))
                        {
                            if (locker.TryEnterWriteLock(timeout))
                            {
                                try
                                {
                                    internalContainer[key] = value;
                                }
                                finally
                                {
                                    locker.ExitWriteLock();
                                }
                            }
                            else
                                throw new TimeoutException();

                            BroadcastChange(new KeyValuePair<TKey, TValue>(key, val), new KeyValuePair<TKey, TValue>(key, value));
                        }
                        else
                        {
                            if (locker.TryEnterWriteLock(timeout))
                            {
                                try
                                {
                                    internalContainer.Add(key, value);
                                }
                                finally
                                {
                                    locker.ExitWriteLock();
                                }
                            }
                            else
                                throw new TimeoutException();

                            BroadcastAdd(new KeyValuePair<TKey, TValue>(key, value));
                        }
                    }
                    finally
                    {
                        if (locker?.IsUpgradeableReadLockHeld ?? false)
                            locker.ExitUpgradeableReadLock();
                    }
                }
                else
                    throw new TimeoutException();
            }
        }
    }
}