// <copyright file="QueueDebugView.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Diagnostics;

namespace IX.Observable
{
    internal sealed class QueueDebugView<T>
    {
        private readonly ObservableQueue<T> queue;

        public QueueDebugView(ObservableQueue<T> queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            this.queue = queue;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[this.queue.InternalContainer.Count];
                this.queue.InternalContainer.CopyTo(items, 0);
                return items;
            }
        }
    }
}