// <copyright file="MultiListListAdapter.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IX.Observable.Adapters
{
    internal class MultiListListAdapter<T> : ListAdapter<T>
    {
        private IList<T> master;
        private List<IEnumerable<T>> slaves;

        internal MultiListListAdapter()
        {
            this.slaves = new List<IEnumerable<T>>();
        }

        public override int Count
        {
            get
            {
                if (this.master == null)
                {
                    throw new InvalidOperationException();
                }

                return this.master.Count + this.slaves.Sum(p => p.Count());
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (this.master == null)
                {
                    throw new InvalidOperationException();
                }

                return this.master.IsReadOnly;
            }
        }

        public override bool IsSynchronized => false;

        public override object SyncRoot => null;

        public override int Add(T item)
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            this.master.Add(item);

            return -1;
        }

        public override void Clear()
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            this.master.Clear();
        }

        public override bool Contains(T item)
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            return this.master.Contains(item) || this.slaves.Any(p => p.Contains(item));
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            int totalCount = this.Count + arrayIndex;
            IEnumerator<T> enumerator = this.GetEnumerator();

            for (int i = arrayIndex; i < totalCount; i++)
            {
                if (!enumerator.MoveNext())
                {
                    break;
                }

                array[i] = enumerator.Current;
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            foreach (T var in this.master)
            {
                yield return var;
            }

            foreach (IEnumerable<T> lst in this.slaves)
            {
                foreach (T var in lst)
                {
                    yield return var;
                }
            }

            yield break;
        }

        public override int Remove(T item)
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            this.master.Remove(item);

            return -1;
        }

        internal void SetMaster<TList>(TList masterList)
            where TList : class, IList<T>, INotifyCollectionChanged
        {
            this.master = masterList ?? throw new ArgumentNullException(nameof(masterList));
        }

        internal void SetSlave<TList>(TList slaveList)
            where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            this.slaves.Add(slaveList ?? throw new ArgumentNullException(nameof(slaveList)));
        }
    }
}