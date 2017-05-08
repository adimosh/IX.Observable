﻿// <copyright file="MultiListMasterSlaveListAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IX.Observable.Adapters
{
    internal class MultiListMasterSlaveListAdapter<T> : ListAdapter<T>
    {
        private IList<T> master;
        private List<IEnumerable<T>> slaves;

        internal MultiListMasterSlaveListAdapter()
        {
            this.slaves = new List<IEnumerable<T>>();
        }

        public override int Count
        {
            get
            {
                this.InitializeMissingMaster();

                return this.master.Count + this.slaves.Sum(p => p.Count());
            }
        }

        public int SlavesCount => this.slaves.Count;

        public override bool IsReadOnly
        {
            get
            {
                this.InitializeMissingMaster();

                return this.master.IsReadOnly;
            }
        }

        internal int MasterCount
        {
            get
            {
                this.InitializeMissingMaster();

                return this.master.Count;
            }
        }

        public override T this[int index]
        {
            get
            {
                this.InitializeMissingMaster();

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

            set
            {
                this.InitializeMissingMaster();

                this.master[index] = value;
            }
        }

        public override int Add(T item)
        {
            this.InitializeMissingMaster();

            this.master.Add(item);

            return this.MasterCount - 1;
        }

        public override void Clear()
        {
            this.InitializeMissingMaster();

            this.master.Clear();
        }

        public override bool Contains(T item)
        {
            this.InitializeMissingMaster();

            return this.master.Contains(item) || this.slaves.Any(p => p.Contains(item));
        }

        public void MasterCopyTo(T[] array, int arrayIndex)
        {
            this.InitializeMissingMaster();

            this.master.CopyTo(array, arrayIndex);
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            this.InitializeMissingMaster();

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
            this.InitializeMissingMaster();

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
            this.InitializeMissingMaster();

            var index = this.master.IndexOf(item);

            this.master.Remove(item);

            return index;
        }

        public override void Insert(int index, T item)
        {
            this.InitializeMissingMaster();

            this.master.Insert(index, item);
        }

        public override int IndexOf(T item)
        {
            this.InitializeMissingMaster();

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
        public override void RemoveAt(int index)
        {
            this.InitializeMissingMaster();

            this.master.RemoveAt(index);
        }

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

        private void InitializeMissingMaster()
        {
            if (this.master == null)
            {
                this.master = new ObservableList<T>();
            }
        }
    }
}