// <copyright file="CollectionDebugView{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IX.Observable.DebugAide
{
    internal sealed class CollectionDebugView<T>
    {
        private readonly ObservableCollectionBase<T> collection;

        public CollectionDebugView(ObservableCollectionBase<T> collection)
        {
            this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[((ICollection<T>)this.collection.InternalContainer).Count];
                this.collection.InternalContainer.CopyTo(items, 0);
                return items;
            }
        }
    }
}