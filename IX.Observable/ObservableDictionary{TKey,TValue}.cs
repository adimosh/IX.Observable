// <copyright file="ObservableDictionary{TKey,TValue}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;
using IX.Observable.SynchronizationLockers;

namespace IX.Observable
{
    /// <summary>
    /// A dictionary that broadcasts its changes.
    /// </summary>
    /// <typeparam name="TKey">The data key type.</typeparam>
    /// <typeparam name="TValue">The data value type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    public class ObservableDictionary<TKey, TValue> : ObservableCollectionBase<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ObservableDictionary()
            : base(new DictionaryCollectionAdapter<TKey, TValue>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(int capacity)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(equalityComparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity, equalityComparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary, comparer)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableDictionary(SynchronizationContext context)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(SynchronizationContext context, int capacity)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(equalityComparer)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(capacity, equalityComparer)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(new DictionaryCollectionAdapter<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary, comparer)), context)
        {
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        public ICollection<TKey> Keys => this.CheckDisposed(() => this.ReadLock(() => ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Keys));

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public ICollection<TValue> Values => this.CheckDisposed(() => this.ReadLock(() =>
            ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Values));

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
            get => this.CheckDisposed(() => this.ReadLock(() => ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary[key]));

            set
            {
                Dictionary<TKey, TValue> dictionary = ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary;

                using (ReadWriteSynchronizationLocker lockingContext = this.ReadWriteLock())
                {
                    if (dictionary.TryGetValue(key, out var val))
                    {
                        lockingContext.Upgrade();
                        dictionary[key] = value;
                    }
                    else
                    {
                        lockingContext.Upgrade();
                        dictionary.Add(key, value);
                    }
                }

                this.BroadcastChange();
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
        public bool ContainsKey(TKey key) => this.CheckDisposed(() => this.ReadLock(() =>
            ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.ContainsKey(key)));

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key)
        {
            this.CheckDisposed();

            bool result;

            using (this.WriteLock())
            {
                result = ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.Remove(key);
            }

            if (result)
            {
                this.BroadcastChange();
            }

            return result;
        }

        /// <summary>
        /// Attempts to fetch a value for a specific key, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully fetched, <c>false</c> otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            this.CheckDisposed();

            using (this.ReadLock())
            {
                return ((DictionaryCollectionAdapter<TKey, TValue>)this.InternalContainer).dictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Called when contents of this dictionary may have changed.
        /// </summary>
        protected override void ContentsMayHaveChanged()
        {
            this.RaisePropertyChanged(nameof(this.Keys));
            this.RaisePropertyChanged(nameof(this.Values));
            this.RaisePropertyChanged(Constants.ItemsName);
        }

        private void BroadcastChange()
        {
            this.RaiseCollectionChanged();
            this.RaisePropertyChanged(nameof(this.Keys));
            this.RaisePropertyChanged(nameof(this.Values));
            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
        }

        protected override void UndoInternally(UndoRedoLevel undoRedoLevel, out Action toInvokeOutsideLock) => throw new NotImplementedException();
        protected override void RedoInternally(UndoRedoLevel undoRedoLevel, out Action toInvokeOutsideLock) => throw new NotImplementedException();
    }
}