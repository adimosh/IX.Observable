// <copyright file="ItemAlreadyCapturedIntoUndoContextException.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace IX.Undoable
{
    /// <summary>
    ///     An exception thrown when the item has already been captured by an undo context.
    /// </summary>
    /// <seealso cref="InvalidOperationException" />
    /// <seealso cref="IUndoableItem" />
    [Serializable]
    [PublicAPI]
    public class ItemAlreadyCapturedIntoUndoContextException : InvalidOperationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemAlreadyCapturedIntoUndoContextException" /> class.
        /// </summary>
        public ItemAlreadyCapturedIntoUndoContextException()
            : base(Resources.ItemAlreadyCapturedIntoUndoContextException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemAlreadyCapturedIntoUndoContextException" /> class.
        /// </summary>
        /// <param name="message">The custom message to display.</param>
        public ItemAlreadyCapturedIntoUndoContextException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemAlreadyCapturedIntoUndoContextException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public ItemAlreadyCapturedIntoUndoContextException(Exception innerException)
            : base(
                Resources.ItemAlreadyCapturedIntoUndoContextException,
                innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemAlreadyCapturedIntoUndoContextException" /> class.
        /// </summary>
        /// <param name="message">The custom message to display.</param>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public ItemAlreadyCapturedIntoUndoContextException(
            string message,
            Exception innerException)
            : base(
                message,
                innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemAlreadyCapturedIntoUndoContextException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected ItemAlreadyCapturedIntoUndoContextException(
            SerializationInfo info,
            StreamingContext context)
            : base(
                info,
                context)
        {
        }
    }
}