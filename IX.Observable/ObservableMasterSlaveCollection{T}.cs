﻿// <copyright file="ObservableMasterSlaveCollection{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{TItem}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ObservableMasterSlaveCollection<T> : ObservableListBase<T>, IList<T>, IReadOnlyCollection<T>, ICollection<T>, ICollection, IList
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

        /// <summary>
        /// Gets the count after an add operation. Used internally.
        /// </summary>
        /// <value>
        /// The count after add.
        /// </value>
        protected override int CountAfterAdd => ((MultiListListAdapter<T>)this.InternalContainer).MasterCount;

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
                this.OnPropertyChanged(Constants.ItemsName);
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
                this.OnPropertyChanged(Constants.ItemsName);
            });
        }

        /// <inheritdoc />
        protected override void ContentsMayHaveChanged() => this.OnPropertyChanged(Constants.ItemsName);
    }
}