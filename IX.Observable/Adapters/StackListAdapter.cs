// <copyright file="StackListAdapter.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections;
using System.Collections.Generic;

namespace IX.Observable.Adapters
{
    internal class StackListAdapter<T> : ListAdapter<T>
    {
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1401 // Fields must be private
        internal Stack<T> stack;
#pragma warning restore SA1401 // Fields must be private
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter

        internal StackListAdapter()
        {
            this.stack = new Stack<T>();
        }

        internal StackListAdapter(Stack<T> stack)
        {
            this.stack = new Stack<T>(stack);
        }

        public override int Count => this.stack.Count;

        public override bool IsReadOnly => false;

        public override bool IsSynchronized => ((ICollection)this.stack).IsSynchronized;

        public override object SyncRoot => ((ICollection)this.stack).SyncRoot;

        public override int Add(T item)
        {
            this.stack.Push(item);
            return this.stack.Count - 1;
        }

        public override void Clear() => this.stack.Clear();

        public override bool Contains(T item) => this.stack.Contains(item);

        public override void CopyTo(T[] array, int arrayIndex) => this.stack.CopyTo(array, arrayIndex);

        public override int Remove(T item) => -1;

        public override IEnumerator<T> GetEnumerator() => this.stack.GetEnumerator();
    }
}