// <copyright file="MultiListListAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
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

        public int SlavesCount => this.slaves.Count;

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

        public override object SyncRoot => (this.master as ICollection)?.SyncRoot;

        internal int MasterCount
        {
            get
            {
                if (this.master == null)
                {
                    throw new InvalidOperationException();
                }

                return this.master.Count;
            }
        }

        public override T this[int index]
        {
            get
            {
                if (index < this.master.Count)
                {
                    return this.master[index];
                }

                var idx = index - this.master.Count;

                foreach (IEnumerable<T> slave in this.slaves)
                {
                    if (slave.Count() <= idx)
                    {
                        idx -= slave.Count();
                        continue;
                    }

                    return slave.ElementAt(idx);
                }

                return default(T);
            }

            set => this.master[index] = value;
        }

        public override int Add(T item)
        {
            if (this.master == null)
            {
                throw new InvalidOperationException();
            }

            this.master.Add(item);

            return this.MasterCount - 1;
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

            var totalCount = this.Count + arrayIndex;
            IEnumerator<T> enumerator = this.GetEnumerator();

            for (var i = arrayIndex; i < totalCount; i++)
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

            var index = this.master.IndexOf(item);

            this.master.Remove(item);

            return index;
        }

        public override void Insert(int index, T item) => this.master.Insert(index, item);

        public override int IndexOf(T item)
        {
            var offset = 0;

            int foundIndex;
            if ((foundIndex = this.master.IndexOf(item)) != -1)
            {
                return foundIndex;
            }
            else
            {
                offset += this.master.Count;

                foreach (List<T> slave in this.slaves.Select(p => p.ToList()))
                {
                    if ((foundIndex = slave.IndexOf(item)) != -1)
                    {
                        return foundIndex + offset;
                    }
                    else
                    {
                        offset += slave.Count();
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Removes an item at a specific index.
        /// </summary>
        /// <param name="index">The index at which to remove from.</param>
        public override void RemoveAt(int index) => this.master.RemoveAt(index);

        internal void SetMaster<TList>(TList masterList)
            where TList : class, IList<T>, INotifyCollectionChanged
        {
            this.master = masterList ?? throw new ArgumentNullException(nameof(masterList));
            masterList.CollectionChanged += this.List_CollectionChanged;
        }

        internal void SetSlave<TList>(TList slaveList)
            where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            this.slaves.Add(slaveList ?? throw new ArgumentNullException(nameof(slaveList)));
            slaveList.CollectionChanged += this.List_CollectionChanged;
        }

        internal void RemoveSlave<TList>(TList slaveList)
            where TList : class, IEnumerable<T>, INotifyCollectionChanged
        {
            try
            {
                slaveList.CollectionChanged -= this.List_CollectionChanged;
            }
            catch
            {
            }

            this.slaves.Remove(slaveList ?? throw new ArgumentNullException(nameof(slaveList)));
        }

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => this.TriggerReset();
    }
}