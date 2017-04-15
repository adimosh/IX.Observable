// <copyright file="ConcurrentObservableDictionary{TKey,TValue}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A dictionary that broadcasts its changes.
    /// </summary>
    /// <typeparam name="TKey">The data key type.</typeparam>
    /// <typeparam name="TValue">The data value type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    public sealed class ConcurrentObservableDictionary<TKey, TValue> : ConcurrentObservableCollectionBase<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ConcurrentObservableDictionary()
            : base(new DictionaryCollectionAdapter<TKey, TValue>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ConcurrentObservableDictionary(int capacity)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(equalityComparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity, equalityComparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ConcurrentObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary, comparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, int capacity)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(equalityComparer)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity, equalityComparer)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ConcurrentObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary, comparer)), context)
        {
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                if (this.SynchronizationLock.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Keys;
                    }
                    finally
                    {
                        this.SynchronizationLock.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                if (this.SynchronizationLock.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Values;
                    }
                    finally
                    {
                        this.SynchronizationLock.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        /// <summary>
        /// Gets or sets the value associated with a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the specified key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (this.SynchronizationLock.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary[key];
                    }
                    finally
                    {
                        this.SynchronizationLock.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }

            set
            {
                Dictionary<TKey, TValue> dictionary = ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary;
                if (this.SynchronizationLock.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        if (dictionary.TryGetValue(key, out var val))
                        {
                            dictionary[key] = value;
                        }
                        else
                        {
                            dictionary.Add(key, value);
                        }
                    }
                    finally
                    {
                        this.SynchronizationLock.ExitWriteLock();
                    }

                    this.BroadcastChange();

                    return;
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value) => this.Add(new KeyValuePair<TKey, TValue>(key, value));

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            if (this.SynchronizationLock.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.ContainsKey(key);
                }
                finally
                {
                    this.SynchronizationLock.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key)
        {
            bool removalResult;
            if (this.SynchronizationLock.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    removalResult = ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Remove(key);
                }
                finally
                {
                    this.SynchronizationLock.ExitWriteLock();
                }
            }
            else
            {
                throw new TimeoutException();
            }

            if (removalResult)
            {
                this.BroadcastChange();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to fetch a value for a specific key, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully fetched, <c>false</c> otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.SynchronizationLock.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.TryGetValue(key, out value);
                }
                finally
                {
                    this.SynchronizationLock.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Called when contents of this dictionary may have changed.
        /// </summary>
        protected sealed override void ContentsMayHaveChanged()
        {
            this.RaisePropertyChanged(nameof(this.Keys));
            this.RaisePropertyChanged(nameof(this.Values));
            this.RaisePropertyChanged(Constants.ItemsName);
        }

        private void BroadcastChange() => this.AsyncPost(() =>
            {
                this.RaiseCollectionChanged();
                this.RaisePropertyChanged(nameof(this.Keys));
                this.RaisePropertyChanged(nameof(this.Values));
                this.RaisePropertyChanged(nameof(this.Count));
                this.RaisePropertyChanged(Constants.ItemsName);
            });
    }
}