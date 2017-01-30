// <copyright file="ObservableListBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for lists that are observable.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{T}" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    /// <seealso cref="System.Collections.Generic.IReadOnlyList{T}" />
    public abstract class ObservableListBase<T> : ObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container.</param>
        /// <param name="context">The context.</param>
        public ObservableListBase(ListAdapter<T> internalContainer, SynchronizationContext context)
            : base(internalContainer, context)
        {
        }

        /// <summary>
        /// Gets the internal list container.
        /// </summary>
        /// <value>
        /// The internal list container.
        /// </value>
        protected ListAdapter<T> InternalListContainer => (ListAdapter<T>)this.InternalContainer;

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at the specified index.</returns>
        public T this[int index] { get => this.InternalListContainer[index]; set => this.InternalListContainer[index] = value; }

        /// <summary>
        /// Determines the index of a specific item, if any.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index of the item, or <c>-1</c> if not found.</returns>
        public int IndexOf(T item) => this.InternalListContainer.IndexOf(item);

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, T item) => this.InternalListContainer.Insert(index, item);

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to remove an item from.</param>
        public void RemoveAt(int index) => this.InternalListContainer.RemoveAt(index);
    }
}