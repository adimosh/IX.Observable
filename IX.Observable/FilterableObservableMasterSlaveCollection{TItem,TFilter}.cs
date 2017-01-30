// <copyright file="FilterableObservableMasterSlaveCollection{TItem,TFilter}.cs" company="Adrian Mos">
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
    public class FilterableObservableMasterSlaveCollection<TItem, TFilter> : ObservableMasterSlaveCollection<TItem>
    {
        private TFilter filter;
        private Func<TItem, TFilter, bool> filteringPredicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterableObservableMasterSlaveCollection{TItem, TFilter}" /> class.
        /// </summary>
        /// <param name="filteringPredicate">The filtering predicate.</param>
        public FilterableObservableMasterSlaveCollection(Func<TItem, TFilter, bool> filteringPredicate)
            : base()
        {
            this.filteringPredicate = filteringPredicate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterableObservableMasterSlaveCollection{TItem, TFilter}"/> class.
        /// </summary>
        /// <param name="filteringPredicate">The filtering predicate.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        public FilterableObservableMasterSlaveCollection(Func<TItem, TFilter, bool> filteringPredicate, SynchronizationContext context)
            : base(context)
        {
            this.filteringPredicate = filteringPredicate;
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

                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(Constants.ItemsName);
                });
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
            var predicate = this.FilteringPredicate;
            if (predicate == null)
            {
                return base.GetEnumerator();
            }

            return this.EnumerateFiltered(predicate);
        }

        /// <inheritdoc/>
        protected override void OnCollectionChangedAdd(TItem addedItem, int index)
        {
            if (EqualityComparer<TFilter>.Default.Equals(this.Filter, default(TFilter)))
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
            if (EqualityComparer<TFilter>.Default.Equals(this.Filter, default(TFilter)))
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
            if (EqualityComparer<TFilter>.Default.Equals(this.Filter, default(TFilter)))
            {
                this.OnCollectionChanged();
            }
            else
            {
                base.OnCollectionChangedRemove(removedItem, index);
            }
        }

        private IEnumerator<TItem> EnumerateFiltered(Func<TItem, TFilter, bool> predicate)
        {
            var filter = this.Filter;

            using (var enumerator = base.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (predicate(current, filter))
                    {
                        yield return current;
                    }
                }
            }

            yield break;
        }
    }
}