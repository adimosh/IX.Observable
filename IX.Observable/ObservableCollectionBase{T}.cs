// <copyright file="ObservableCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
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
    public abstract class ObservableCollectionBase<T> : ObservableReadOnlyCollectionBase<T>, ICollection<T>
    {
        private object resetCountLocker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext context)
            : base(internalContainer, context)
        {
            this.InternalContainer = internalContainer;
            this.resetCountLocker = new object();
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
    }
}