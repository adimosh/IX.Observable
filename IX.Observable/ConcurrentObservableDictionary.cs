// <copyright file="ConcurrentObservableDictionary.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A thread-safe dictionary that broadcasts its changes.
    /// </summary>
    /// <typeparam name="TKey">The data key type.</typeparam>
    /// <typeparam name="TValue">The data value type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    public class ConcurrentObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        private bool disposedValue = false; // To detect redundant calls

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, int capacity)
            : base(context, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IEqualityComparer<TKey> equalityComparer)
            : base(context, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(context, capacity, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary)
            : base(context, dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(context, dictionary, comparer)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        ~ConcurrentObservableDictionary()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(false);
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempts to fetch a value for a specific key in a thread-safe manner, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully fetched, <c>false</c> otherwise.</returns>
        public override bool TryGetValue(TKey key, out TValue value)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.TryGetValue(key, out value);
                }
                finally
                {
                    this.locker.ExitReadLock();
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (!this.locker.TryEnterReadLock(this.timeout))
            {
                throw new TimeoutException();
            }

            KeyValuePair<TKey, TValue>[] array;

            try
            {
                array = new KeyValuePair<TKey, TValue>[this.InternalContainer.Count];
                ((ICollection<KeyValuePair<TKey, TValue>>)this.InternalContainer).CopyTo(array, 0);
            }
            finally
            {
                this.locker.ExitReadLock();
            }

            foreach (var kvp in array)
            {
                yield return kvp;
            }

            yield break;
        }

        /// <summary>
        /// Copies the elements of the dictionary into an array, at the specified index.
        /// </summary>
        /// <param name="array">The array to copy elements into.</param>
        /// <param name="arrayIndex">The index at which to start copying items.</param>
        public override void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    base.CopyTo(array, arrayIndex);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public override bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.Contains(item);
                }
                finally
                {
                    this.locker.ExitReadLock();
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterReadLock(this.timeout))
            {
                try
                {
                    return base.ContainsKey(key);
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="disposing"><c>true</c> for normal disposal, where normal operation should dispose sub-objects,
        /// <c>false</c> for a GC disposal without the normal pattern.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.locker.Dispose();
                }

                this.InternalContainer.Clear();
                this.InternalContainer = null;

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Adds an item to the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected override void AddInternal(TKey key, TValue value)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterUpgradeableReadLock(this.timeout))
            {
                try
                {
                    if (this.InternalContainer.ContainsKey(key))
                    {
                        throw new ArgumentException(Resources.DictionaryItemAlreadyExists, nameof(key));
                    }

                    if (this.locker.TryEnterWriteLock(this.timeout))
                    {
                        try
                        {
                            this.InternalContainer.Add(key, value);
                        }
                        finally
                        {
                            this.locker.ExitWriteLock();
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
                finally
                {
                    if (this.locker.IsUpgradeableReadLockHeld)
                    {
                        this.locker.ExitUpgradeableReadLock();
                    }
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Clears all items from the dictionary (internal overridable procedure).
        /// </summary>
        protected override void ClearInternal()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    base.ClearInternal();
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterUpgradeableReadLock(this.timeout))
            {
                try
                {
                    if (!this.InternalContainer.TryGetValue(key, out value))
                    {
                        return false;
                    }

                    if (this.locker.TryEnterWriteLock(this.timeout))
                    {
                        try
                        {
                            return this.InternalContainer.Remove(key);
                        }
                        finally
                        {
                            this.locker.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    if (this.locker.IsUpgradeableReadLockHeld)
                    {
                        this.locker.ExitUpgradeableReadLock();
                    }
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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentObservableDictionary<TKey, TValue>));
            }

            if (this.locker.TryEnterWriteLock(this.timeout))
            {
                try
                {
                    return ((ICollection<KeyValuePair<TKey, TValue>>)this.InternalContainer).Remove(item);
                }
                finally
                {
                    this.locker.ExitWriteLock();
                }
            }

            throw new TimeoutException();
        }
    }
}