using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableQueue<T> : ObservableCollectionBase, IQueue<T>, IEnumerable<T>, ICollection
    {
        /// <summary>
        /// The data container of the observable queue.
        /// </summary>
        protected Queue<T> internalContainer;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        public ObservableQueue()
        {
            internalContainer = new Queue<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ObservableQueue(IEnumerable<T> collection)
        {
            internalContainer = new Queue<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ObservableQueue(int capacity)
        {
            internalContainer = new Queue<T>(capacity);
        }
        #endregion

        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return internalContainer.Count;
            }
        }

        /// <summary>
        /// Gets an enumerator for the observable queue.
        /// </summary>
        /// <returns>The queue enumerator.</returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return internalContainer.GetEnumerator();
        }

        #region Explicit implementations
        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)internalContainer).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)internalContainer).SyncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)internalContainer).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Clears the queue of all its objects.
        /// </summary>
        public void Clear()
        {
            ClearInternal();

            Task.Run(() =>
            {
                OnCollectionChanged();
                OnPropertyChanged(nameof(Count));
            });
        }

        /// <summary>
        /// Clears the queue of all its objects (internal overridable procedure).
        /// </summary>
        protected virtual void ClearInternal()
        {
            internalContainer.Clear();
        }

        /// <summary>
        /// Determines whether the queue contains a specific item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the queue contains a specific item, <c>false</c> otherwise.</returns>
        public virtual bool Contains(T item)
        {
            return internalContainer.Contains(item);
        }

        /// <summary>
        /// Copies the contents of the queue to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy the items into.</param>
        /// <param name="arrayIndex">The index at which to start in the array.</param>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            internalContainer.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Dequeues and removes an item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public T Dequeue()
        {
            T item = DequeueInternal();

            Task.Run(() =>
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: item, oldIndex: 0);
                OnPropertyChanged(nameof(Count));
            });

            return item;
        }

        /// <summary>
        /// Dequeues and removes an item from the queue (internal overridable procedure).
        /// </summary>
        /// <returns>The dequeued item.</returns>
        protected virtual T DequeueInternal()
        {
            return internalContainer.Dequeue();
        }

        /// <summary>
        /// Enqueues an item into the queue.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            EnqueueInternal(item);

            Task.Run(() =>
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: item, newIndex: 0);
                OnPropertyChanged(nameof(Count));
            });
        }

        /// <summary>
        /// Enqueues an item into the queue (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        protected virtual void EnqueueInternal(T item)
        {
            internalContainer.Enqueue(item);
        }

        /// <summary>
        /// Peeks at the topmost item in the queue without dequeueing it.
        /// </summary>
        /// <returns>The topmost item in the queue.</returns>
        public virtual T Peek()
        {
            return internalContainer.Peek();
        }

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public virtual T[] ToArray()
        {
            return internalContainer.ToArray();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess()
        {
            internalContainer.TrimExcess();
        }
    }
}