using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IX.Observable
{
    /// <summary>
    /// A stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableStack<T> : ObservableCollectionBase, IStack<T>, IReadOnlyCollection<T>, ICollection
    {
        /// <summary>
        /// The data container for the observable stack.
        /// </summary>
        protected Stack<T> internalContainer;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        public ObservableStack()
            : base(null)
        {
            internalContainer = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(int capacity)
            : base(null)
        {
            internalContainer = new Stack<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(IEnumerable<T> collection)
            : base(null)
        {
            internalContainer = new Stack<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableStack(SynchronizationContext context)
            : base(context)
        {
            internalContainer = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(SynchronizationContext context, int capacity)
            : base(context)
        {
            internalContainer = new Stack<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(SynchronizationContext context, IEnumerable<T> collection)
            : base(context)
        {
            internalContainer = new Stack<T>(collection);
        }
        #endregion

        #region IReadOnlyCollection
        /// <summary>
        /// The number of elements in the observable stack.
        /// </summary>
        public virtual int Count
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
                return Count;
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
        public virtual IEnumerator<T> GetEnumerator()
        {
            return internalContainer.GetEnumerator();
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

        #region IStack
        /// <summary>
        /// Clears the observable stack.
        /// </summary>
        public void Clear()
        {
            ClearInternal();

            if (CollectionChangedEmpty() && PropertyChangedEmpty())
                return;

            Task.Run(() =>
            {
                OnCollectionChanged();
                OnPropertyChanged(nameof(Count));
            });
        }

        /// <summary>
        /// Clears the observable stack (internal overridable procedure).
        /// </summary>
        protected void ClearInternal()
        {
            internalContainer.Clear();
        }

        /// <summary>
        /// Checks whether or not a certain item is in the stack.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns><c>true</c> if the item was found, <c>false</c> otherwise.</returns>
        public virtual bool Contains(T item)
        {
            return internalContainer.Contains(item);
        }

        /// <summary>
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public virtual T Peek()
        {
            return internalContainer.Peek();
        }

        /// <summary>
        /// Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            T item = PopInternal();

            var st = new Tuple<T, int>(item, internalContainer.Count);

            Task.Run(() =>
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: st.Item1, oldIndex: st.Item2);
                OnPropertyChanged(nameof(Count));
            });

            return item;
        }

        /// <summary>
        /// Pops the topmost element from the stack, removing it (internal overridable procedure).
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        protected virtual T PopInternal() => internalContainer.Pop();

        /// <summary>
        /// Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            internalContainer.Push(item);

            Task.Run(() =>
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: item);
                OnPropertyChanged(nameof(Count));
            });
        }

        /// <summary>
        /// Pushes an element to the top of the stack (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to push.</param>
        protected virtual void PushInternal(T item) => internalContainer.Push(item);

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public virtual T[] ToArray()
        {
            return internalContainer.ToArray();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess()
        {
            internalContainer.TrimExcess();
        }
        #endregion
    }
}