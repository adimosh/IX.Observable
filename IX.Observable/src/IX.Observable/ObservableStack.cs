using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace IX.Observable
{
    /// <summary>
    /// A stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    public class ObservableStack<T> : ObservableCollectionBase, IStack<T>, IReadOnlyCollection<T>, ICollection
    {
        private Stack<T> internalContainer;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        public ObservableStack()
        {
            internalContainer = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(int capacity)
        {
            internalContainer = new Stack<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(IEnumerable<T> collection)
        {
            internalContainer = new Stack<T>(collection);
        }
        #endregion

        #region IReadOnlyCollection
        /// <summary>
        /// The number of elements in the observable stack.
        /// </summary>
        public int Count
        {
            get
            {
                return internalContainer.Count;
            }
        }

        int ICollection.Count
        {
            get
            {
                return internalContainer.Count;
            }
        }

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

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return internalContainer.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)internalContainer).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalContainer.GetEnumerator();
        }
        #endregion

        #region IStack
        /// <summary>
        /// Clears the observable stack.
        /// </summary>
        public void Clear()
        {
            internalContainer.Clear();

            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            OnPropertyChanged(nameof(Count));
        }

        /// <summary>
        /// Checks whether or not a certain item is in the stack.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns><c>true</c> if the item was found, <c>false</c> otherwise.</returns>
        public bool Contains(T item)
        {
            return internalContainer.Contains(item);
        }

        /// <summary>
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Peek()
        {
            return internalContainer.Peek();
        }

        /// <summary>
        /// Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            T item = internalContainer.Pop();

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems: new List<T> { item });
            OnPropertyChanged(nameof(Count));

            return item;
        }

        /// <summary>
        /// Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            internalContainer.Push(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, newItems: new List<T> { item });
            OnPropertyChanged(nameof(Count));
        }

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public T[] ToArray()
        {
            return internalContainer.ToArray();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            internalContainer.TrimExcess();
        }
        #endregion
    }
}