// <copyright file="PropertyStateChange.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using JetBrains.Annotations;

namespace IX.Undoable
{
    /// <summary>
    ///     A state change of a property.
    /// </summary>
    [PublicAPI]
    public abstract class PropertyStateChange : StateChange
    {
        /// <summary>
        ///     Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }
    }
}