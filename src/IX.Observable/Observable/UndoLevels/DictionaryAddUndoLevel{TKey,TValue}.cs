// <copyright file="DictionaryAddUndoLevel{TKey,TValue}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using IX.Undoable;
using JetBrains.Annotations;

namespace IX.Observable.UndoLevels
{
    /// <summary>
    ///     An undo level for adding something in a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="IX.Undoable.StateChange" />
    [PublicAPI]
    public class DictionaryAddUndoLevel<TKey, TValue> : StateChange
    {
        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public TKey Key { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public TValue Value { get; set; }
    }
}