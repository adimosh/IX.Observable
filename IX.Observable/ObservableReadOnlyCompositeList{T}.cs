﻿// <copyright file="ObservableReadOnlyCompositeList{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// An observable, composite, thread-safe and readonly list made of multiple lists of the same rank.
    /// </summary>
    /// <typeparam name="T">The type of the list item.</typeparam>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="IX.Observable.ObservableReadOnlyCollectionBase{T}" />
    public class ObservableReadOnlyCompositeList<T> : ObservableReadOnlyCollectionBase<T>
    {
        private ReaderWriterLockSlim locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCompositeList{T}"/> class.
        /// </summary>
        public ObservableReadOnlyCompositeList()
            : base(new MultiListListAdapter<T>(), null)
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCompositeList{T}"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        public ObservableReadOnlyCompositeList(SynchronizationContext context)
            : base(new MultiListListAdapter<T>(), context)
        {
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Sets a list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void SetList<TList>(TList list)
            where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            using (this.WriteLock())
            {
                ((MultiListListAdapter<T>)this.InternalContainer).SetList(list);
            }

            this.RaiseCollectionChanged();
            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
        }

        /// <summary>
        /// Removes a list.
        /// </summary>
        /// <typeparam name="TList">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        public void RemoveList<TList>(TList list)
            where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            using (this.WriteLock())
            {
                ((MultiListListAdapter<T>)this.InternalContainer).RemoveList(list);
            }

            this.RaiseCollectionChanged();
            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
        }

        /// <summary>
        /// Disposes of this instance and performs necessary cleanup.
        /// </summary>
        /// <param name="managedDispose">Indicates whether or not the call came from <see cref="System.IDisposable"/> or from the destructor.</param>
        protected override void Dispose(bool managedDispose)
        {
            if (managedDispose)
            {
                this.locker.Dispose();
            }

            base.Dispose(managedDispose);

            this.locker = null;
        }
    }
}