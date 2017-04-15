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
        /// <remarks>
        /// <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public virtual void Add(T item)
        {
            int newIndex;
            using (this.WriteLock())
            {
                newIndex = this.InternalContainer.Add(item);
            }

            if (newIndex == -1)
            {
                this.RaiseCollectionChanged();
            }
            else
            {
                this.RaiseCollectionChangedAdd(item, newIndex);
            }

            this.RaisePropertyChanged(nameof(this.Count));
            this.ContentsMayHaveChanged();
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <remarks>
        /// <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public virtual void Clear()
        {
            using (this.WriteLock())
            {
                this.InternalContainer.Clear();
            }

            this.RaiseCollectionChanged();
            this.RaisePropertyChanged(nameof(this.Count));
            this.ContentsMayHaveChanged();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="ObservableCollectionBase{T}" />; otherwise, <c>false</c>.
        /// This method also returns false if <paramref name="item" /> is not found in the original <see cref="ObservableCollectionBase{T}" />.
        /// </returns>
        /// <remarks>
        /// <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public virtual bool Remove(T item)
        {
            int oldIndex;
            using (this.WriteLock())
            {
                oldIndex = this.InternalContainer.Remove(item);
            }

            if (oldIndex >= 0)
            {
                this.RaiseCollectionChangedRemove(item, oldIndex);
                this.RaisePropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();

                return true;
            }
            else if (oldIndex < -1)
            {
                this.RaiseCollectionChanged();
                this.RaisePropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}