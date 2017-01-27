﻿// <copyright file="ObservableCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for collections that are observable.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
#pragma warning disable SA1402 // File may only contain a single type
    public abstract class ObservableCollectionBase<T> : ObservableCollectionBase, ICollection<T>, IReadOnlyCollection<T>, ICollection
#pragma warning restore SA1402 // File may only contain a single type
    {
        private CollectionAdapter<T> internalContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = internalContainer;
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public virtual int Count => ((IReadOnlyCollection<T>)this.InternalContainer).Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => this.InternalContainer.IsReadOnly;

        /// <summary>
        /// Gets a value indicating whether this instance is synchronized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
        /// </value>
        public bool IsSynchronized => this.InternalContainer.IsSynchronized;

        /// <summary>
        /// Gets the synchronize root.
        /// </summary>
        /// <value>
        /// The synchronize root.
        /// </value>
        public object SyncRoot => this.InternalContainer.SyncRoot;

        /// <summary>
        /// Gets or sets the internal object container.
        /// </summary>
        /// <value>
        /// The internal container.
        /// </value>
        protected internal CollectionAdapter<T> InternalContainer
        {
            get => this.internalContainer;
            set
            {
                if (this.internalContainer != null)
                {
                    this.internalContainer.MustReset -= this.InternalContainer_MustReset;
                }

                this.internalContainer = value;

                if (this.internalContainer != null)
                {
                    this.internalContainer.MustReset += this.InternalContainer_MustReset;
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableCollectionBase{T}" />.</param>
        public virtual void Add(T item)
        {
            var newIndex = this.InternalContainer.Add(item);

            this.AsyncPost(
                (state) =>
                {
                    if (state.index == -1)
                    {
                        this.OnCollectionChanged();
                    }
                    else
                    {
                        this.OnCollectionChangedAdd(state.item, state.index);
                    }

                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                }, new { index = newIndex, item });
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        public virtual void Clear()
        {
            this.InternalContainer.Clear();

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();
            });
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollectionBase{T}" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ObservableCollectionBase{T}" />; otherwise, false.
        /// </returns>
        public virtual bool Contains(T item) => this.InternalContainer.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollectionBase{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public virtual void CopyTo(T[] array, int arrayIndex) => this.InternalContainer.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator() => this.InternalContainer.GetEnumerator();

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="ObservableCollectionBase{T}" />; otherwise, <c>false</c>. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ObservableCollectionBase{T}" />.
        /// </returns>
        public virtual bool Remove(T item)
        {
            var oldIndex = this.InternalContainer.Remove(item);

            if (oldIndex >= 0)
            {
                this.AsyncPost(
                    (state) =>
                    {
                        this.OnCollectionChangedRemove(state.item, state.index);
                        this.OnPropertyChanged(nameof(this.Count));
                        this.ContentsMayHaveChanged();
                    }, new { index = oldIndex, item });
                return true;
            }
            else if (oldIndex < -1)
            {
                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Copies the contents of the container to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">Index of the array.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            T[] tempArray = new T[this.Count - index];
            this.CopyTo(tempArray, index);
            tempArray.CopyTo(array, index);
        }

        /// <summary>
        /// Called when an item is added to a collection.
        /// </summary>
        /// <param name="addedItem">The added item.</param>
        /// <param name="index">The index.</param>
        protected virtual void OnCollectionChangedAdd(T addedItem, int index)
            => this.OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: addedItem, newIndex: index);

        /// <summary>
        /// Called when an item in a collection is changed.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="index">The index.</param>
        protected virtual void OnCollectionChangedChanged(T oldItem, T newItem, int index)
            => this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, newItem, index, index);

        /// <summary>
        /// Called when an item is removed from a collection.
        /// </summary>
        /// <param name="removedItem">The removed item.</param>
        /// <param name="index">The index.</param>
        protected virtual void OnCollectionChangedRemove(T removedItem, int index)
            => this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: removedItem, oldIndex: index);

        /// <summary>
        /// Called when the contents may have changed so that proper notifications can happen.
        /// </summary>
        protected virtual void ContentsMayHaveChanged()
        {
        }

        private void InternalContainer_MustReset(object sender, EventArgs e) => this.AsyncPost(() =>
        {
            this.OnCollectionChanged();
            this.OnPropertyChanged(nameof(this.Count));
            this.ContentsMayHaveChanged();
        });
    }
}