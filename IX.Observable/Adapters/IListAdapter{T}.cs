﻿// <copyright file="IListAdapter{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Collections;
using System.Collections.Generic;

namespace IX.Observable.Adapters
{
    /// <summary>
    /// An adapter interface for non-standard collection types.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    /// <seealso cref="ICollectionAdapter{T}" />
    /// <seealso cref="System.Collections.IList" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    /// <seealso cref="System.Collections.Generic.IReadOnlyList{T}" />
    public interface IListAdapter<T> : ICollectionAdapter<T>, IList<T>, IReadOnlyList<T>, IList
    {
    }
}