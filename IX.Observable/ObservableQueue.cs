// <copyright file="ObservableQueue.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A queue that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(QueueDebugView<>))]
    public class ObservableQueue<T> : ObservableCollectionBase, IQueue<T>, IEnumerable<T>, ICollection
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// The data container of the observable queue.
        /// </summary>
        protected internal Queue<T> InternalContainer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        public ObservableQueue()
            : base(null)
        {
            this.InternalContainer = new Queue<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy from.</param>
        public ObservableQueue(IEnumerable<T> collection)
            : base(null)
        {
            this.InternalContainer = new Queue<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ObservableQueue(int capacity)
            : base(null)
        {
            this.InternalContainer = new Queue<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableQueue(SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = new Queue<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy from.</param>
        public ObservableQueue(SynchronizationContext context, IEnumerable<T> collection)
            : base(context)
        {
            this.InternalContainer = new Queue<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableQueue{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public ObservableQueue(SynchronizationContext context, int capacity)
            : base(context)
        {
            this.InternalContainer = new Queue<T>(capacity);
        }

        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return this.InternalContainer.Count;
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
        /// Gets an enumerator for the observable queue.
        /// </summary>
        /// <returns>The queue enumerator.</returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return this.InternalContainer.GetEnumerator();
        }

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index) => ((ICollection)this.InternalContainer).CopyTo(array, index);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Clears the queue of all its objects.
        /// </summary>
        public void Clear()
        {
            this.ClearInternal();

            this.AsyncPost(() =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged();
            });
        }

        /// <summary>
        /// Determines whether the queue contains a specific item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the queue contains a specific item, <c>false</c> otherwise.</returns>
        public virtual bool Contains(T item) => this.InternalContainer.Contains(item);

        /// <summary>
        /// Copies the contents of the queue to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy the items into.</param>
        /// <param name="arrayIndex">The index at which to start in the array.</param>
        public virtual void CopyTo(T[] array, int arrayIndex) => this.InternalContainer.CopyTo(array, arrayIndex);

        /// <summary>
        /// Dequeues and removes an item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public T Dequeue()
        {
            T item = this.DequeueInternal();

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: state, oldIndex: 0);
            }, item);

            return item;
        }

        /// <summary>
        /// Enqueues an item into the queue.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            this.EnqueueInternal(item);

            var st = new Tuple<T, int>(item, this.Count - 1);

            this.AsyncPost(
                (state) =>
            {
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: state.Item1, newIndex: st.Item2);
            }, st);
        }

        /// <summary>
        /// Peeks at the topmost item in the queue without dequeueing it.
        /// </summary>
        /// <returns>The topmost item in the queue.</returns>
        public virtual T Peek() => this.InternalContainer.Peek();

        /// <summary>
        /// Copies the items of the queue into a new array.
        /// </summary>
        /// <returns>An array of items that are contained in the queue.</returns>
        public virtual T[] ToArray() => this.InternalContainer.ToArray();

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ObservableQueue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess() => this.InternalContainer.TrimExcess();

        /// <summary>
        /// Clears the queue of all its objects (internal overridable procedure).
        /// </summary>
        protected virtual void ClearInternal()
        {
            var st = this.InternalContainer;
            this.InternalContainer = new Queue<T>();

            Task.Run(() => st.Clear());
        }

        /// <summary>
        /// Enqueues an item into the queue (internal overridable procedure).
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        protected virtual void EnqueueInternal(T item) => this.InternalContainer.Enqueue(item);

        /// <summary>
        /// Dequeues and removes an item from the queue (internal overridable procedure).
        /// </summary>
        /// <returns>The dequeued item.</returns>
        protected virtual T DequeueInternal() => this.InternalContainer.Dequeue();
    }
}