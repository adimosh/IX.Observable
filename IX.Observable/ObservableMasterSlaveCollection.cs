// <copyright file="ObservableMasterSlaveCollection.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{TItem}" />
    public class ObservableMasterSlaveCollection<T> : ObservableCollectionBase<T>, IList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableMasterSlaveCollection{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        public ObservableMasterSlaveCollection(SynchronizationContext context)
            : base(new MultiListListAdapter<T>(), context)
        {
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get => ((MultiListListAdapter<T>)this.InternalContainer)[index];
            set
            {
                ((MultiListListAdapter<T>)this.InternalContainer)[index] = value;

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.OnPropertyChanged("Item[]");
                });
            }
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            return ((MultiListListAdapter<T>)this.InternalContainer).IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            ((MultiListListAdapter<T>)this.InternalContainer).Insert(item, index);

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            });
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            ((MultiListListAdapter<T>)this.InternalContainer).RemoveAt(index);

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            });
        }

        /// <inheritdoc />
        protected override void ContentsMayHaveChanged()
        {
            this.OnPropertyChanged("Item[]");
        }
    }
}