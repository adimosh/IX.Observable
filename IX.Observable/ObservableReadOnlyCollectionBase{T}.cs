// <copyright file="ObservableReadOnlyCollectionBase{T}.cs" company="Adrian Mos">
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
    /// A base class for read-only collections that are observable.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public class ObservableReadOnlyCollectionBase<T> : ObservableCollectionBase, IReadOnlyCollection<T>, ICollection
    {
        private CollectionAdapter<T> internalContainer;
        private object resetCountLocker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableReadOnlyCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = internalContainer;
            this.resetCountLocker = new object();
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
        /// Gets the ignore reset count.
        /// </summary>
        /// <value>
        /// The ignore reset count.
        /// </value>
        /// <remarks>
        /// <para>If this count is any number greater than zero, the <see cref="CollectionAdapter{T}.MustReset"/> event will be ignored.</para>
        /// <para>Each invocation of the collection adapter's <see cref="CollectionAdapter{T}.MustReset"/> event will decrease this counter by one until zero.</para>
        /// </remarks>
        protected int IgnoreResetCount { get; private set; }

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
        /// Increases the <see cref="IgnoreResetCount"/> by one.
        /// </summary>
        protected void IncreaseIgnoreMustResetCounter()
        {
            lock (this.resetCountLocker)
            {
                this.IgnoreResetCount++;
            }
        }

        /// <summary>
        /// Increases the <see cref="IgnoreResetCount"/> by one.
        /// </summary>
        /// <param name="increaseBy">The amount to increase by.</param>
        protected void IncreaseIgnoreMustResetCounter(int increaseBy)
        {
            lock (this.resetCountLocker)
            {
                this.IgnoreResetCount += increaseBy;
            }
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
            bool shouldReset;
            lock (this.resetCountLocker)
            {
                if (this.IgnoreResetCount > 0)
                {
                    this.IgnoreResetCount--;
                    shouldReset = false;
                }
                else
                {
                    shouldReset = true;
                }
            }

            if (shouldReset)
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();
            }
        });
    }
}