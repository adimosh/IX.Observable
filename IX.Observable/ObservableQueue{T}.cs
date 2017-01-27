// <copyright file="ObservableQueue{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(QueueDebugView<>))]
    public class ObservableQueue<T> : ObservableCollectionBase<T>, IQueue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        public ObservableQueue()
            : base(new QueueListAdapter<T>(new Queue<T>()), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ObservableQueue(IEnumerable<T> collection)
            : base(new QueueListAdapter<T>(new Queue<T>(collection)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ObservableQueue(int capacity)
            : base(new QueueListAdapter<T>(new Queue<T>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableQueue(SynchronizationContext context)
            : base(new QueueListAdapter<T>(new Queue<T>()), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy from.</param>
        public ObservableQueue(SynchronizationContext context, IEnumerable<T> collection)
            : base(new QueueListAdapter<T>(new Queue<T>(collection)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ObservableQueue(SynchronizationContext context, int capacity)
            : base(new QueueListAdapter<T>(new Queue<T>(capacity)), context)
        {
        }

        /// <summary>
        /// Dequeues and removes an item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public T Dequeue()
        {
            T item = ((QueueListAdapter<T>)this.InternalContainer).queue.Dequeue();

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChangedRemove(state, 0);
            }, item);

            return item;
        }

        /// <summary>
        /// Enqueues an item into the queue.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            ((QueueListAdapter<T>)this.InternalContainer).queue.Enqueue(item);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChangedAdd(state.item, state.index);
            }, new { index = this.Count - 1, item });
        }

        /// <summary>
        /// Peeks at the topmost item in the queue without dequeueing it.
        /// </summary>
        /// <returns>The topmost item in the queue.</returns>
        public virtual T Peek() => ((QueueListAdapter<T>)this.InternalContainer).queue.Peek();

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public virtual T[] ToArray() => ((QueueListAdapter<T>)this.InternalContainer).queue.ToArray();

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess() => ((QueueListAdapter<T>)this.InternalContainer).queue.TrimExcess();
    }
}