// <copyright file="ICollectionAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace IX.Observable.Adapters
{
    /// <summary>
    /// An adapter interface for non-standard collection types.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    /// <seealso cref="ICollection{T}" />
    /// <seealso cref="ICollection" />
    public interface ICollectionAdapter<T> : ICollection<T>
    {
        /// <summary>
        /// Occurs when the owner of this list adapter must reset.
        /// </summary>
        event EventHandler MustReset;

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index at which the item was added, or -1 if not applicable.</returns>
        new int Add(T item);

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index at which the item resided before being removed, or -1 if not applicable.</returns>
        new int Remove(T item);
    }
}