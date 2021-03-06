// <copyright file="SubItemStateChange.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using JetBrains.Annotations;

namespace IX.Undoable
{
    /// <summary>
    ///     A state change belonging to a sub-object.
    /// </summary>
    [PublicAPI]
    public class SubItemStateChange : StateChange
    {
        /// <summary>
        ///     Gets or sets the state changes belonging to the sub-object.
        /// </summary>
        public StateChange[] StateChanges { get; set; }

        /// <summary>
        ///     Gets or sets the instance of the sub-object.
        /// </summary>
        public IUndoableItem SubObject { get; set; }
    }
}