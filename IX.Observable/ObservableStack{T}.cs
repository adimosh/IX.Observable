// <copyright file="ObservableStack{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    public class ObservableStack<T> : ObservableCollectionBase<T>, IStack<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        public ObservableStack()
            : base(new StackCollectionAdapter<T>(new Stack<T>()), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(int capacity)
            : base(new StackCollectionAdapter<T>(new Stack<T>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(IEnumerable<T> collection)
            : base(new StackCollectionAdapter<T>(new Stack<T>(collection)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableStack(SynchronizationContext context)
            : base(new StackCollectionAdapter<T>(new Stack<T>()), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(SynchronizationContext context, int capacity)
            : base(new StackCollectionAdapter<T>(new Stack<T>(capacity)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(SynchronizationContext context, IEnumerable<T> collection)
            : base(new StackCollectionAdapter<T>(new Stack<T>(collection)), context)
        {
        }

        /// <summary>
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public virtual T Peek() => ((StackCollectionAdapter<T>)this.InternalContainer).stack.Peek();

        /// <summary>
        /// Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            T item = ((StackCollectionAdapter<T>)this.InternalContainer).stack.Pop();

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged(Constants.ItemsName);
                this.OnCollectionChangedRemove(state.item, state.index);
            }, new { index = this.Count, item });

            return item;
        }

        /// <summary>
        /// Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            ((StackCollectionAdapter<T>)this.InternalContainer).stack.Push(item);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged(Constants.ItemsName);
                this.OnCollectionChangedAdd(state.item, state.index);
            }, new { index = this.Count - 1, item });
        }

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public virtual T[] ToArray() => ((StackCollectionAdapter<T>)this.InternalContainer).stack.ToArray();

        /// <summary>
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess() => ((StackCollectionAdapter<T>)this.InternalContainer).stack.TrimExcess();
    }
}