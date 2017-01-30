// <copyright file="ObservableMasterSlaveCollection{T}.cs" company="Adrian Mos">
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
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{TItem}" />
    public class ObservableMasterSlaveCollection<T> : ObservableCollectionBase<T>, IList<T>, IReadOnlyCollection<T>, ICollection<T>, ICollection, IList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableMasterSlaveCollection{T}"/> class.
        /// </summary>
        public ObservableMasterSlaveCollection()
            : base(new MultiListListAdapter<T>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableMasterSlaveCollection{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        public ObservableMasterSlaveCollection(SynchronizationContext context)
            : base(new MultiListListAdapter<T>(), context)
        {
        }

        /// <inheritdoc />
        public bool IsFixedSize => false;

        /// <inheritdoc />
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
        public void CopyTo(Array array, int index) => ((ICollection)this.InternalContainer).CopyTo(array, index);

        /// <inheritdoc />
        public int IndexOf(T item) => ((MultiListListAdapter<T>)this.InternalContainer).IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            ((MultiListListAdapter<T>)this.InternalContainer).Insert(index, item);

            this.AsyncPost(
                (state) =>
            {
                this.OnCollectionChangedAdd(state.item, state.index);
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            }, new { item, index });
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            var container = (MultiListListAdapter<T>)this.InternalContainer;
            var item = container[index];
            container.RemoveAt(index);

            this.AsyncPost(
                (state) =>
            {
                this.OnCollectionChangedRemove(state.item, state.index);
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            }, new { index, item });
        }

        /// <summary>
        /// Sets the master list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void SetMasterList<TList>(TList list)
                    where TList : class, IList<T>, INotifyCollectionChanged
        {
            ((MultiListListAdapter<T>)this.InternalContainer).SetMaster(list);

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            });
        }

        /// <summary>
        /// Sets a master list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void SetSlaveList<TList>(TList list)
                    where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            ((MultiListListAdapter<T>)this.InternalContainer).SetSlave(list);

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.OnPropertyChanged("Item[]");
            });
        }

        /// <inheritdoc />
        int IList.Add(object value)
        {
            if (value is T v)
            {
                this.Add(v);

                return ((MultiListListAdapter<T>)this.InternalContainer).MasterCount - 1;
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        bool IList.Contains(object value)
        {
            if (value is T v)
            {
                return this.Contains(v);
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        int IList.IndexOf(object value)
        {
            if (value is T v)
            {
                return this.IndexOf(v);
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        void IList.Insert(int index, object value)
        {
            if (value is T v)
            {
                this.Insert(index, v);

                return;
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        void IList.Remove(object value)
        {
            if (value is T v)
            {
                this.Remove(v);

                return;
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        protected override void ContentsMayHaveChanged() => this.OnPropertyChanged("Item[]");
    }
}