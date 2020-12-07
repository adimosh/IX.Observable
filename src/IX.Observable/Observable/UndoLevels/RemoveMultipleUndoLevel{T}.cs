// <copyright file="RemoveMultipleUndoLevel{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using IX.Undoable;
using JetBrains.Annotations;

namespace IX.Observable.UndoLevels
{
    /// <summary>
    ///     An undo step for when some items were removed.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <seealso cref="IX.Undoable.StateChange" />
    [PublicAPI]
    public class RemoveMultipleUndoLevel<T> : StateChange
    {
        /// <summary>
        ///     Gets or sets the removed items.
        /// </summary>
        /// <value>The removed items.</value>
        public T[] RemovedItems { get; set; }

        /// <summary>
        ///     Gets or sets the indexes of the removed items.
        /// </summary>
        /// <value>The indexes.</value>
        public int[] Indexes { get; set; }
    }
}