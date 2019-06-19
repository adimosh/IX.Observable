// <copyright file="KVP{TKey,TValue}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Diagnostics;
using JetBrains.Annotations;

namespace IX.Observable.DebugAide
{
    /// <summary>
    ///     A debug view of a key-value pair in an observable dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [DebuggerDisplay("[{Key}] = \"{Value}\"")]
    [UsedImplicitly]
    public sealed class Kvp<TKey, TValue>
    {
        /// <summary>
        ///     Gets the key.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [UsedImplicitly]
        public TKey Key { get; internal set; }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [UsedImplicitly]
        public TValue Value { get; internal set; }
    }
}