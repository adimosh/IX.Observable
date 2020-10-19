// <copyright file="QueueDebugView{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace IX.Observable.DebugAide
{
    /// <summary>
    ///     A debug view for an observable queue.
    /// </summary>
    /// <typeparam name="T">The type of object in the queue.</typeparam>
    [UsedImplicitly]
    public sealed class QueueDebugView<T>
    {
        private readonly ObservableQueue<T> queue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueDebugView{T}" /> class.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <exception cref="ArgumentNullException">queue is null.</exception>
        [UsedImplicitly]
        public QueueDebugView(ObservableQueue<T> queue)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        [UsedImplicitly]
        public T[] Items
        {
            get
            {
                var items = new T[((ICollection<T>)this.queue.InternalContainer).Count];
                this.queue.InternalContainer.CopyTo(
                    items,
                    0);
                return items;
            }
        }
    }
}