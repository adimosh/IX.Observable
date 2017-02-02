﻿// <copyright file="ConcurrentFilterableObservableMasterSlaveCollection{TItem,TFilter}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.DebugAide;

namespace IX.Observable
{
    /// <summary>
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections, whose items can also be filtered.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <seealso cref="IX.Observable.ObservableMasterSlaveCollection{T}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ConcurrentFilterableObservableMasterSlaveCollection<TItem, TFilter> : ConcurrentObservableMasterSlaveCollection<TItem>
    {
        private TFilter filter;
        private Func<TItem, TFilter, bool> filteringPredicate;
        private IList<TItem> cachedFilteredElements;
        private ReaderWriterLockSlim cacheLocker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentFilterableObservableMasterSlaveCollection{TItem, TFilter}" /> class.
        /// </summary>
        /// <param name="filteringPredicate">The filtering predicate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filteringPredicate"/> is <c>null</c> (<c>Nothing</c>) in Visual Basic.</exception>
        public ConcurrentFilterableObservableMasterSlaveCollection(Func<TItem, TFilter, bool> filteringPredicate)
            : base()
        {
            this.filteringPredicate = filteringPredicate ?? throw new ArgumentNullException(nameof(filteringPredicate));
            this.cacheLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentFilterableObservableMasterSlaveCollection{TItem, TFilter}"/> class.
        /// </summary>
        /// <param name="filteringPredicate">The filtering predicate.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filteringPredicate"/> is <c>null</c> (<c>Nothing</c>) in Visual Basic.</exception>
        public ConcurrentFilterableObservableMasterSlaveCollection(Func<TItem, TFilter, bool> filteringPredicate, SynchronizationContext context)
            : base(context)
        {
            this.filteringPredicate = filteringPredicate ?? throw new ArgumentNullException(nameof(filteringPredicate));
            this.cacheLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Gets the filtering predicate.
        /// </summary>
        /// <value>
        /// The filtering predicate.
        /// </value>
        public Func<TItem, TFilter, bool> FilteringPredicate => this.filteringPredicate;

        /// <summary>
        /// Gets or sets the filter value.
        /// </summary>
        /// <value>
        /// The filter value.
        /// </value>
        public TFilter Filter
        {
            get => this.filter;
            set
            {
                this.filter = value;

                this.ClearCachedContents();

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.OnPropertyChanged(Constants.ItemsName);
                });
            }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        /// <value>
        /// The item count.
        /// </value>
        public override int Count
        {
            get
            {
                if (this.IsFilter())
                {
                    return this.CheckAndCache().Count;
                }

                return base.Count;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<TItem> GetEnumerator()
        {
            if (this.IsFilter())
            {
                return this.CheckAndCache().GetEnumerator();
            }

            return base.GetEnumerator();
        }

        /// <inheritdoc/>
        protected override void OnCollectionChangedAdd(TItem addedItem, int index)
        {
            if (this.IsFilter())
            {
                this.OnCollectionChanged();
            }
            else
            {
                base.OnCollectionChangedAdd(addedItem, index);
            }
        }

        /// <inheritdoc/>
        protected override void OnCollectionChangedChanged(TItem oldItem, TItem newItem, int index)
        {
            if (this.IsFilter())
            {
                this.OnCollectionChanged();
            }
            else
            {
                base.OnCollectionChangedChanged(oldItem, newItem, index);
            }
        }

        /// <inheritdoc/>
        protected override void OnCollectionChangedRemove(TItem removedItem, int index)
        {
            if (this.IsFilter())
            {
                this.OnCollectionChanged();
            }
            else
            {
                base.OnCollectionChangedRemove(removedItem, index);
            }
        }

        private IEnumerator<TItem> EnumerateFiltered()
        {
            var filter = this.Filter;

            using (var enumerator = base.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (this.filteringPredicate(current, filter))
                    {
                        yield return current;
                    }
                }
            }

            yield break;
        }

        private void FillCachedList()
        {
        }

        private IList<TItem> CheckAndCache()
        {
            if (this.cacheLocker.TryEnterUpgradeableReadLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    if (this.cachedFilteredElements == null)
                    {
                        if (this.cacheLocker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
                        {
                            try
                            {
                                this.cachedFilteredElements = new List<TItem>(this.InternalListContainer.Count);

                                using (var enumerator = this.EnumerateFiltered())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var current = enumerator.Current;
                                        this.cachedFilteredElements.Add(current);
                                    }
                                }
                            }
                            finally
                            {
                                this.cacheLocker.ExitWriteLock();
                            }

                            return this.cachedFilteredElements;
                        }

                        throw new TimeoutException();
                    }
                    else
                    {
                        return this.cachedFilteredElements;
                    }
                }
                finally
                {
                    if (this.cacheLocker.IsUpgradeableReadLockHeld)
                    {
                        this.cacheLocker.ExitUpgradeableReadLock();
                    }
                }
            }

            throw new TimeoutException();
        }

        private void ClearCachedContents()
        {
            if (this.cacheLocker.TryEnterWriteLock(Constants.ConcurrentLockAcquisitionTimeout))
            {
                try
                {
                    if (this.cachedFilteredElements != null)
                    {
                        var coll = this.cachedFilteredElements;
                        this.cachedFilteredElements = null;
                        coll.Clear();
                    }
                }
                finally
                {
                    this.cacheLocker.ExitWriteLock();
                }

                return;
            }

            throw new TimeoutException();
        }

        private bool IsFilter() => !EqualityComparer<TFilter>.Default.Equals(this.Filter, default(TFilter));
    }
}