using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IX.Observable
{
    /// <summary>
    /// A dictionary that broadcasts its changes.
    /// </summary>
    /// <typeparam name="TKey">The data key type.</typeparam>
    /// <typeparam name="TValue">The data value type.</typeparam>
    public class ObservableDictionary<TKey, TValue> : ObservableCollectionBase, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> internalContainer;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        public ObservableDictionary()
        {
            internalContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public ObservableDictionary(int capacity)
        {
            internalContainer = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
        {
            internalContainer = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        /// <param name="equalityComparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
        {
            internalContainer = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            internalContainer = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of items to copy from.</param>
        /// <param name="comparer">A comparer object to use for equality comparison.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            internalContainer = new Dictionary<TKey, TValue>(dictionary, comparer);
        }
        #endregion

        #region IDictionary
        /// <summary>
        /// Gets or sets the value associated with a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the specified key.</returns>
        public TValue this[TKey key]
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

                    ChangeUtility(new KeyValuePair<TKey, TValue>(key, val), new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    AddUtility(new KeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs in the dictionary.
        /// </summary>
        public int Count
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
        public ICollection<TKey> Keys
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
                return internalContainer.Keys;
            }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public ICollection<TValue> Values
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
                return internalContainer.Values;
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The key/value pair.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (internalContainer.ContainsKey(item.Key))
                throw new ArgumentException("An item with the same key already exists in this dictionary.", nameof(item));

            AddUtility(item);
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            if (internalContainer.ContainsKey(key))
                throw new ArgumentException("An item with the same key already exists in this dictionary.", nameof(key));

            AddUtility(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Clears all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            internalContainer.Clear();

            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(Count));
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return internalContainer.Contains(item);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><c>true</c> whether a key has been found, <c>false</c> otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            return internalContainer.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the dictionary into an array, at the specified index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>An enumerator of key/value pairs.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
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
            if (((ICollection<KeyValuePair<TKey, TValue>>)internalContainer).Remove(item))
            {
                RemoveUtility(item);
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
            TValue value;
            if (internalContainer.TryGetValue(key, out value) && internalContainer.Remove(key))
            {
                RemoveUtility(new KeyValuePair<TKey, TValue>(key, value));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to fetch a value for a specific key, indicating whether it has been found or not.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return internalContainer.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalContainer.GetEnumerator();
        }
        #endregion

        #region Utility
        private void AddUtility(KeyValuePair<TKey, TValue> item)
        {
            internalContainer.Add(item.Key, item.Value);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, newItems: new List<KeyValuePair<TKey, TValue>> { item });
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(Count));
        }

        private void RemoveUtility(KeyValuePair<TKey, TValue> item)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems: new List<KeyValuePair<TKey, TValue>> { item });
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(Count));
        }

        private void ChangeUtility(KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
        {
            OnCollectionChanged(
                NotifyCollectionChangedAction.Replace,
                oldItems: new List<KeyValuePair<TKey, TValue>> { oldItem },
                newItems: new List<KeyValuePair<TKey, TValue>> { newItem });
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
        }
        #endregion
    }
}