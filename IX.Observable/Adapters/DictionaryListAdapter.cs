// <copyright file="DictionaryListAdapter.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IX.Observable.Adapters
{
    internal class DictionaryListAdapter<TKey, TValue> : CollectionAdapter<KeyValuePair<TKey, TValue>>
    {
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1401 // Fields must be private
        internal Dictionary<TKey, TValue> dictionary;
#pragma warning restore SA1401 // Fields must be private
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter

        internal DictionaryListAdapter()
        {
            this.dictionary = new Dictionary<TKey, TValue>();
        }

        internal DictionaryListAdapter(Dictionary<TKey, TValue> dictionary)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public override int Count => this.dictionary.Count;

        public override bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).IsReadOnly;

        public override bool IsSynchronized => ((ICollection)this.dictionary).IsSynchronized;

        public override object SyncRoot => ((ICollection)this.dictionary).SyncRoot;

        public override int Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Add(item);
            return -1;
        }

        public override void Clear()
        {
            var tempdict = this.dictionary;
            this.dictionary = new Dictionary<TKey, TValue>();

            var syncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            Task.Run(() =>
            {
                this.dictionary.Clear();
            }).ConfigureAwait(false);
            SynchronizationContext.SetSynchronizationContext(syncContext);
        }

        public override bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Contains(item);

        public override void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).CopyTo(array, arrayIndex);

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this.dictionary.GetEnumerator();

        public override int Remove(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Remove(item);
            return -1;
        }
    }
}