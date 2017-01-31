// <copyright file="ConcurrentObservableListBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for lists that are observable and concurrent.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ConcurrentObservableCollectionBase{T}" />
    /// <seealso cref="IList" />
    /// <seealso cref="IList{T}" />
    /// <seealso cref="IReadOnlyList{T}" />
    public class ConcurrentObservableListBase<T> : ConcurrentObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>, IList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObservableListBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container.</param>
        /// <param name="context">The context.</param>
        public ConcurrentObservableListBase(ListAdapter<T> internalContainer, SynchronizationContext context)
            : base(internalContainer, context)
        {
        }

        /// <summary>
        /// Gets a value indicating whether or not this list is of a fixed size.
        /// </summary>
        public virtual bool IsFixedSize => this.InternalListContainer.IsFixedSize;

        /// <summary>
        /// Gets the internal list container.
        /// </summary>
        /// <value>
        /// The internal list container.
        /// </value>
        protected ListAdapter<T> InternalListContainer => (ListAdapter<T>)this.InternalContainer;

        /// <summary>
        /// Gets the count after an add operation. Used internally.
        /// </summary>
        /// <value>
        /// The count after add.
        /// </value>
        protected virtual int CountAfterAdd => this.Count;

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at the specified index.</returns>
        public virtual T this[int index]
        {
            get
            {
                if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    try
                    {
                        return this.InternalListContainer[index];
                    }
                    finally
                    {
                        this.Locker.ExitReadLock();
                    }
                }

                throw new TimeoutException();
            }

            set
            {
                if (index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
                {
                    T oldValue;
                    try
                    {
                        oldValue = this.InternalListContainer[index];
                        this.InternalListContainer[index] = value;
                    }
                    finally
                    {
                        this.Locker.ExitWriteLock();
                    }

                    this.AsyncPost(
                        (state) =>
                        {
                            this.OnCollectionChangedChanged(state.OldValue, state.NewValue, state.Index);
                            this.OnPropertyChanged(nameof(this.Count));
                            this.ContentsMayHaveChanged();
                        }, new { OldValue = oldValue, NewValue = value, Index = index });

                    return;
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at the specified index.</returns>
        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T v)
                {
                    this[index] = v;

                    return;
                }

                throw new InvalidCastException();
            }
        }

        /// <summary>
        /// Determines the index of a specific item, if any.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index of the item, or <c>-1</c> if not found.</returns>
        public virtual int IndexOf(T item)
        {
            if (this.Locker.TryEnterReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    return this.InternalListContainer.IndexOf(item);
                }
                finally
                {
                    this.Locker.ExitReadLock();
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="item">The item.</param>
        public virtual void Insert(int index, T item)
        {
            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    this.InternalListContainer.Insert(index, item);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.OnCollectionChangedAdd(state.NewValue, state.Index);
                        this.OnPropertyChanged(nameof(this.Count));
                        this.ContentsMayHaveChanged();
                    }, new { NewValue = item, Index = index });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to remove an item from.</param>
        public virtual void RemoveAt(int index)
        {
            if (index >= this.Count)
            {
                return;
            }

            if (this.Locker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                T item;
                try
                {
                    item = this.InternalListContainer[index];
                    this.InternalListContainer.RemoveAt(index);
                }
                finally
                {
                    this.Locker.ExitWriteLock();
                }

                this.AsyncPost(
                    (state) =>
                    {
                        this.OnCollectionChangedRemove(state.NewValue, state.Index);
                        this.OnPropertyChanged(nameof(this.Count));
                        this.ContentsMayHaveChanged();
                    }, new { NewValue = item, Index = index });

                return;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableListBase{T}" />.
        /// </summary>
        /// <param name="value">The object to add to the <see cref="ObservableListBase{T}" />.</param>
        /// <returns>The index at which the item was added.</returns>
        int IList.Add(object value)
        {
            if (value is T v)
            {
                this.Add(v);

                return this.CountAfterAdd - 1;
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableListBase{T}" /> contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="ObservableListBase{T}" />.</param>
        /// <returns>
        /// true if <paramref name="value" /> is found in the <see cref="ObservableListBase{T}" />; otherwise, false.
        /// </returns>
        bool IList.Contains(object value)
        {
            if (value is T v)
            {
                return this.Contains(v);
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// Determines the index of a specific item, if any.
        /// </summary>
        /// <param name="value">The item value.</param>
        /// <returns>The index of the item, or <c>-1</c> if not found.</returns>
        int IList.IndexOf(object value)
        {
            if (value is T v)
            {
                return this.IndexOf(v);
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="value">The item value.</param>
        void IList.Insert(int index, object value)
        {
            if (value is T v)
            {
                this.Insert(index, v);

                return;
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableListBase{T}" />.
        /// </summary>
        /// <param name="value">The object value to remove from the <see cref="ObservableListBase{T}" />.</param>
        void IList.Remove(object value)
        {
            if (value is T v)
            {
                this.Remove(v);

                return;
            }

            throw new InvalidCastException();
        }
    }
}