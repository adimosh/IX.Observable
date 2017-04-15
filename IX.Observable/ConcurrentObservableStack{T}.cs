// <copyright file="ConcurrentObservableStack{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// A thread-safe stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    public class ConcurrentObservableStack<T> : ConcurrentObservableCollectionBase<T>, IStack<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        public ConcurrentObservableStack()
            : base(new StackCollectionAdapter<T>(new Stack<T>()), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(int capacity)
            : base(new StackCollectionAdapter<T>(new Stack<T>(capacity)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(IEnumerable<T> collection)
            : base(new StackCollectionAdapter<T>(new Stack<T>(collection)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableStack(SynchronizationContext context)
            : base(new StackCollectionAdapter<T>(new Stack<T>()), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(SynchronizationContext context, int capacity)
            : base(new StackCollectionAdapter<T>(new Stack<T>(capacity)), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableStack{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(SynchronizationContext context, IEnumerable<T> collection)
            : base(new StackCollectionAdapter<T>(new Stack<T>(collection)), context)
        {
        }

        /// <summary>
        /// Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public virtual T Peek()
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((StackCollectionAdapter<T>)this.InternalContainer).stack.Peek();
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                T item;
                try
                {
                    item = ((StackCollectionAdapter<T>)this.InternalContainer).stack.Pop();
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.RaisePropertyChanged(nameof(this.Count));
                        this.RaisePropertyChanged(Constants.ItemsName);
                        this.OnCollectionChangedRemove(state.item, state.index);
                    }, new { index = this.Count, item });

                return item;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((StackCollectionAdapter<T>)this.InternalContainer).stack.Push(item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.RaisePropertyChanged(nameof(this.Count));
                        this.RaisePropertyChanged(Constants.ItemsName);
                        this.OnCollectionChangedRemove(state.item, state.index);
                    }, new { index = this.Count - 1, item });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public virtual T[] ToArray()
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return ((StackCollectionAdapter<T>)this.InternalContainer).stack.ToArray();
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current capacity.
        /// </summary>
        public virtual void TrimExcess()
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    ((StackCollectionAdapter<T>)this.InternalContainer).stack.TrimExcess();
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                return;
            }

            throw new TimeoutException();
        }
    }
}