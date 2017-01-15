// <copyright file="ObservableStack.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

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
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    public class ObservableStack<T> : ObservableCollectionBase, IStack<T>, IReadOnlyCollection<T>, ICollection
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// The data container for the observable stack.
        /// </summary>
        protected internal Stack<T> InternalContainer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        public ObservableStack()
            : base(null)
        {
            this.InternalContainer = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(int capacity)
            : base(null)
        {
            this.InternalContainer = new Stack<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(IEnumerable<T> collection)
            : base(null)
        {
            this.InternalContainer = new Stack<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableStack(SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(SynchronizationContext context, int capacity)
            : base(context)
        {
            this.InternalContainer = new Stack<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(SynchronizationContext context, IEnumerable<T> collection)
            : base(context)
        {
            this.InternalContainer = new Stack<T>(collection);
        }

        /// <summary>
        /// Gets the number of elements in the observable stack.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return this.InternalContainer.Count;
            }
        }

        /// <summary>
        /// Gets the number of elements in the observable stack.
        /// </summary>
        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        /// <inheritdoc/>
        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)this.InternalContainer).IsSynchronized;
            }
        }

        /// <inheritdoc/>
        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)this.InternalContainer).SyncRoot;
            }
        }

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public virtual IEnumerator<T> GetEnumerator() => this.InternalContainer.GetEnumerator();

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index) => ((ICollection)this.InternalContainer).CopyTo(array, index);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Clears the observable stack.
        /// </summary>
        public void Clear()
        {
            this.ClearInternal();

            if (this.CollectionChangedEmpty() && this.PropertyChangedEmpty())
            {
                return;
            }

            this.AsyncPost(() =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged();
            });
        }

        /// <summary>
        /// Checks whether or not a certain item is in the stack.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns><c>true</c> if the item was found, <c>false</c> otherwise.</returns>
        public virtual bool Contains(T item) => this.InternalContainer.Contains(item);

        /// <summary>
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public virtual T Peek() => this.InternalContainer.Peek();

        /// <summary>
        /// Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            T item = this.PopInternal();

            var st = new Tuple<T, int>(item, this.Count);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: state.Item1, oldIndex: state.Item2);
            }, st);

            return item;
        }

        /// <summary>
        /// Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            this.PushInternal(item);

            var st = new Tuple<T, int>(item, this.Count - 1);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: state.Item1, newIndex: state.Item2);
            }, st);
        }

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public virtual T[] ToArray() => this.InternalContainer.ToArray();

        /// <summary>
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess() => this.InternalContainer.TrimExcess();

        /// <summary>
        /// Clears the observable stack (internal overridable procedure).
        /// </summary>
        protected void ClearInternal()
        {
            var st = this.InternalContainer;
            this.InternalContainer = new Stack<T>();

            Task.Run(() => st.Clear());
        }

        /// <summary>
        /// Pushes an element to the top of the stack (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to push.</param>
        protected virtual void PushInternal(T item) => this.InternalContainer.Push(item);

        /// <summary>
        /// Pops the topmost element from the stack, removing it (internal overridable procedure).
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        protected virtual T PopInternal() => this.InternalContainer.Pop();
    }
}