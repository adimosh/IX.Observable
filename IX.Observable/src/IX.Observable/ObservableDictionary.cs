using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        /// <summary>
        /// The data container of the observable dictionary.
        /// </summary>
        protected internal Dictionary<TKey, TValue> internalContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ObservableDictionary()
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(int capacity)
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(null)
        {
            internalContainer = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableDictionary(SynchronizationContext context)
            : base(context)
        {
            internalContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(SynchronizationContext context, int capacity)
            : base(context)
        {
            internalContainer = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(SynchronizationContext context, IEqualityComparer<TKey> equalityComparer)
            : base(context)
        {
            internalContainer = new Dictionary<TKey, TValue>(equalityComparer);
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
            internalContainer = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(SynchronizationContext context, IDictionary<TKey, TValue> dictionary)
            : base(context)
        {
            internalContainer = new Dictionary<TKey, TValue>(dictionary);
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
            internalContainer = new Dictionary<TKey, TValue>(dictionary, comparer);
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
                return internalContainer[key];
            }
            set
            {
                TValue val;
                if (internalContainer.TryGetValue(key, out val))
                {
                    internalContainer[key] = value;

                    BroadcastChange(new KeyValuePair<TKey, TValue>(key, val), new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    internalContainer.Add(key, value);

                    BroadcastAdd(new KeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs in the dictionary.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return internalContainer.Count;
            }
        }

        /// <summary>
        /// Gets whether or not this instance of a dictionary is readonly.
        /// </summary>
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
                return internalContainer.Keys;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public virtual ICollection<TValue> Values
        {
            get
            {
                return internalContainer.Values;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The key/value pair.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddInternal(item.Key, item.Value);

            BroadcastAdd(item);
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            AddInternal(key, value);

            BroadcastAdd(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Adds an item to the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected virtual void AddInternal(TKey key, TValue value)
        {
            if (internalContainer.ContainsKey(key))
                throw new ArgumentException(Resources.DictionaryItemAlreadyExists, nameof(key));

            internalContainer.Add(key, value);
        }

        /// <summary>
        /// Clears all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            ClearInternal();

            BroadcastReset();
        }

        /// <summary>
        /// Clears all items from the dictionary (internal overridable procedure).
        /// </summary>
        protected virtual void ClearInternal()
        {
            var st = internalContainer;
            internalContainer = new Dictionary<TKey, TValue>();

            Task.Run(() => st.Clear());
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return internalContainer.Contains(item);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return internalContainer.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the dictionary into an array, at the specified index.
        /// </summary>
        /// <param name="array">The array to copy elements into.</param>
        /// <param name="arrayIndex">The index at which to start copying items.</param>
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return internalContainer.GetEnumerator();
        }

        /// <summary>
        /// Attempts to remove a key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (RemoveInternal(item))
            {
                BroadcastRemove(item, 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove a key/value pair (internal overridable procedure).
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected virtual bool RemoveInternal(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).Remove(item);
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key)
        {
            TValue value;
            if (RemoveInternal(key, out value))
            {
                BroadcastRemove(new KeyValuePair<TKey, TValue>(key, value), 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove all info related to a key from the dictionary (internal overridable procedure).
        /// </summary>
        /// <param name="key">The key to remove data from.</param>
        /// <param name="value">The value that was removed, if any.</param>
        /// <returns><c>true</c> if the removal was successful, <c>false</c> otherwise.</returns>
        protected virtual bool RemoveInternal(TKey key, out TValue value)
        {
            return internalContainer.TryGetValue(key, out value) && internalContainer.Remove(key);
        }

        /// <summary>
        /// Attempts to fetch a value for a specific key, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully fetched, <c>false</c> otherwise.</returns>
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return internalContainer.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Broadcasts an &quot;add&quot; change.
        /// </summary>
        /// <param name="item">The added item.</param>
        protected void BroadcastAdd(KeyValuePair<TKey, TValue> item)
        {
            if (CollectionChangedEmpty() && PropertyChangedEmpty())
                return;

            var st = new Tuple<KeyValuePair<TKey, TValue>, int>(item, 0);

            AsyncPost((state) =>
            {
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: state.Item1, newIndex: state.Item2);
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;remove&quot; change.
        /// </summary>
        /// <param name="item">The removed item.</param>
        /// <param name="index">The removed index (mandatory for remove change).</param>
        protected void BroadcastRemove(KeyValuePair<TKey, TValue> item, int index)
        {
            if (CollectionChangedEmpty() && PropertyChangedEmpty())
                return;

            var st = new Tuple<KeyValuePair<TKey, TValue>, int>(item, index);

            AsyncPost((state) =>
            {
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: state.Item1, oldIndex: state.Item2);
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;change&quot; change.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        protected void BroadcastChange(KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
        {
            if (CollectionChangedEmpty() && PropertyChangedEmpty())
                return;

            var st = new Tuple<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>>(oldItem, newItem);

            AsyncPost((state) =>
            {
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged("Item[]");

                var array = new KeyValuePair<TKey, TValue>[Count];
                CopyTo(array, 0);

                int index = Array.IndexOf(array, state.Item2);
                if (index != -1)
                {
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem: state.Item1, newItem: state.Item2, newIndex: index);
                }
            }, st);
        }

        /// <summary>
        /// Broadcasts a &quot;reset&quot; change.
        /// </summary>
        protected void BroadcastReset()
        {
            if (CollectionChangedEmpty() && PropertyChangedEmpty())
                return;

            AsyncPost(() =>
            {
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged();
            });
        }
    }
    internal sealed class DictionaryDebugView<TKey, TValue>
    {
        private readonly ObservableDictionary<TKey, TValue> dict;

        public DictionaryDebugView(ObservableDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KVP<TKey, TValue>[] Items
        {
            get
            {
                KeyValuePair<TKey, TValue>[] items = new KeyValuePair<TKey, TValue>[dict.internalContainer.Count];
                ((ICollection<KeyValuePair<TKey, TValue>>)dict.internalContainer).CopyTo(items, 0);
                return items.Select(p => new KVP<TKey, TValue> { Key = p.Key, Value = p.Value }).ToArray();
            }
        }
    }

    [DebuggerDisplay("[{Key}] = \"{Value}\"")]
    internal sealed class KVP<TKey, TValue>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public TKey Key { get; internal set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public TValue Value { get; internal set; }
    }
}