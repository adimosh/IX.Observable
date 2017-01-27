// <copyright file="ListAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace IX.Observable.Adapters
{
    /// <summary>
    /// An adapter class for various list types.
    /// </summary>
    /// <typeparam name="T">The type of items in the adapter.</typeparam>
    /// <seealso cref="IX.Observable.Adapters.CollectionAdapter{T}" />
    /// <seealso cref="IX.Observable.Adapters.IListAdapter{T}" />
    public abstract class ListAdapter<T> : CollectionAdapter<T>, IListAdapter<T>
    {
        public abstract T this[int index] { get; set; }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T)
                {
                    this[index] = (T)value;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }

        T IReadOnlyList<T>.this[int index] => this[index];

        public abstract bool IsFixedSize { get; }

        public abstract int Add(object value);

        public abstract bool Contains(object value);

        public abstract int IndexOf(object value);

        public abstract int IndexOf(T item);

        public abstract void Insert(int index, object value);

        public abstract void Insert(int index, T item);

        public abstract void Remove(object value);

        public abstract void RemoveAt(int index);
    }
}