// <copyright file="QueueCollectionAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections;
using System.Collections.Generic;

namespace IX.Observable.Adapters
{
    internal class QueueCollectionAdapter<T> : CollectionAdapter<T>
    {
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1401 // Fields must be private
        internal Queue<T> queue;
#pragma warning restore SA1401 // Fields must be private
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter

        internal QueueCollectionAdapter()
        {
            this.queue = new Queue<T>();
        }

        internal QueueCollectionAdapter(Queue<T> queue)
        {
            this.queue = new Queue<T>(queue);
        }

        public override int Count => this.queue.Count;

        public override bool IsReadOnly => false;

        public override bool IsSynchronized => ((ICollection)this.queue).IsSynchronized;

        public override object SyncRoot => ((ICollection)this.queue).SyncRoot;

        public override int Add(T item)
        {
            this.queue.Enqueue(item);
            return this.queue.Count - 1;
        }

        public override void Clear() => this.queue.Clear();

        public override bool Contains(T item) => this.queue.Contains(item);

        public override void CopyTo(T[] array, int arrayIndex) => this.queue.CopyTo(array, arrayIndex);

        public override int Remove(T item) => -1;

        public override IEnumerator<T> GetEnumerator() => this.queue.GetEnumerator();
    }
}