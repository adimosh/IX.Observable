// <copyright file="ObservableDictionary.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class ObservableDictionary<TKey, TValue> : ObservableCollectionBase, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// The data container of the observable dictionary.
        /// </summary>
        protected internal Dictionary<TKey, TValue> InternalContainer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ObservableDictionary()
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(int capacity)
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(null)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableDictionary(SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(SynchronizationContext context, int capacity)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, IEqualityComparer<TKey> equalityComparer)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(context)
        {
            this.InternalContainer = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Gets the number of key/value pairs in the dictionary.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return this.InternalContainer.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this instance of a dictionary is readonly.
        /// </summary>
        /// <remarks>
        /// <para>This will always return <c>false</c>.</para>
        /// </remarks>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        public virtual ICollection<TKey> Keys
        {
            get
            {
                return this.InternalContainer.Keys;
            }
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public virtual ICollection<TValue> Values
        {
            get
            {
                return this.InternalContainer.Values;
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return this.Values;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual TValue this[TKey key]
        {
            get
            {
                return this.InternalContainer[key];
            }

            set
            {
                if (this.InternalContainer.TryGetValue(key, out var val))
                {
                    this.InternalContainer[key] = value;

                    this.BroadcastChange(new KeyValuePair<TKey, TValue>(key, val), new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    this.InternalContainer.Add(key, value);

                    this.BroadcastAdd(new KeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The key/value pair.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.AddInternal(item.Key, item.Value);

            this.BroadcastAdd(item);
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            this.AddInternal(key, value);

            this.BroadcastAdd(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Clears all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            this.ClearInternal();

            this.BroadcastReset();
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.InternalContainer.Contains(item);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return this.InternalContainer.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the dictionary into an array, at the specified index.
        /// </summary>
        /// <param name="array">The array to copy elements into.</param>
        /// <param name="arrayIndex">The index at which to start copying items.</param>
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)this.InternalContainer).CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.InternalContainer.GetEnumerator();
        }

        /// <summary>
        /// Attempts to remove a key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.RemoveInternal(item))
            {
                this.BroadcastRemove(item, 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key)
        {
            if (this.RemoveInternal(key, out var value))
            {
                this.BroadcastRemove(new KeyValuePair<TKey, TValue>(key, value), 0);
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
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return this.InternalContainer.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <param name="value">The value that was removed, if any.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected virtual bool RemoveInternal(TKey key, out TValue value) => this.InternalContainer.TryGetValue(key, out value) && this.InternalContainer.Remove(key);

        /// <summary>
        /// Attempts to remove a key/value pair (internal overridable procedure).
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected virtual bool RemoveInternal(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this.InternalContainer).Remove(item);

        /// <summary>
        /// Adds an item to the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected virtual void AddInternal(TKey key, TValue value)
        {
            if (this.InternalContainer.ContainsKey(key))
            {
                throw new ArgumentException(Resources.DictionaryItemAlreadyExists, nameof(key));
            }

            this.InternalContainer.Add(key, value);
        }

        /// <summary>
        /// Clears all items from the dictionary (internal overridable procedure).
        /// </summary>
        protected virtual void ClearInternal()
        {
            var st = this.InternalContainer;
            this.InternalContainer = new Dictionary<TKey, TValue>();

            Task.Run(() => st.Clear());
        }

        /// <summary>
        /// Broadcasts an &quot;add&quot; change.
        /// </summary>
        /// <param name="item">The added item.</param>
        protected void BroadcastAdd(KeyValuePair<TKey, TValue> item)
        {
            if (this.CollectionChangedEmpty() && this.PropertyChangedEmpty())
            {
                return;
            }

            var st = new Tuple<KeyValuePair<TKey, TValue>, int>(item, 0);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Keys));
                this.OnPropertyChanged(nameof(this.Values));
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: state.Item1, newIndex: state.Item2);
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;remove&quot; change.
        /// </summary>
        /// <param name="item">The removed item.</param>
        /// <param name="index">The removed index (mandatory for remove change).</param>
        protected void BroadcastRemove(KeyValuePair<TKey, TValue> item, int index)
        {
            if (this.CollectionChangedEmpty() && this.PropertyChangedEmpty())
            {
                return;
            }

            var st = new Tuple<KeyValuePair<TKey, TValue>, int>(item, index);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Keys));
                this.OnPropertyChanged(nameof(this.Values));
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: state.Item1, oldIndex: state.Item2);
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;change&quot; change.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        protected void BroadcastChange(KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
        {
            if (this.CollectionChangedEmpty() && this.PropertyChangedEmpty())
            {
                return;
            }

            var st = new Tuple<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>>(oldItem, newItem);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Keys));
                this.OnPropertyChanged(nameof(this.Values));
                this.OnPropertyChanged("Item[]");

                var array = new KeyValuePair<TKey, TValue>[this.Count];
                this.CopyTo(array, 0);

                int index = Array.IndexOf(array, state.Item2);
                if (index != -1)
                {
                    this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem: state.Item1, newItem: state.Item2, newIndex: index);
                }
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;reset&quot; change.
        /// </summary>
        protected void BroadcastReset()
        {
            if (this.CollectionChangedEmpty() && this.PropertyChangedEmpty())
            {
                return;
            }

            this.AsyncPost(() =>
            {
                this.OnPropertyChanged(nameof(this.Keys));
                this.OnPropertyChanged(nameof(this.Values));
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged();
            });
        }
    }
}