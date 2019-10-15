// <copyright file="AutoReleaseTransactionContext.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using IX.Guaranteed;
using IX.StandardExtensions.Contracts;
using IX.StandardExtensions.Extensions;
using IX.Undoable;

namespace IX.Observable
{
    /// <summary>
    ///     An auto-capture-releasing class that captures in a transaction.
    /// </summary>
    internal class AutoReleaseTransactionContext : OperationTransaction
    {
        private readonly EventHandler<EditCommittedEventArgs> editableHandler;
        private readonly IUndoableItem item;
        private readonly IUndoableItem[] items;
        private readonly IUndoableItem parentContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoReleaseTransactionContext" /> class.
        /// </summary>
        public AutoReleaseTransactionContext()
        {
            this.Success();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoReleaseTransactionContext" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="editableHandler">The editable handler.</param>
        public AutoReleaseTransactionContext(
            IUndoableItem item,
            IUndoableItem parentContext,
            EventHandler<EditCommittedEventArgs> editableHandler)
        {
            // Contract validation
            Contract.RequiresNotNullPrivate(
                in item,
                nameof(item));
            Contract.RequiresNotNullPrivate(
                in parentContext,
                nameof(parentContext));
            Contract.RequiresNotNullPrivate(
                in editableHandler,
                nameof(editableHandler));

            // Data validation
            if (!item.IsCapturedIntoUndoContext || item.ParentUndoContext != parentContext)
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            // State
            this.items = null;
            this.item = item;
            this.editableHandler = editableHandler;
            this.parentContext = parentContext;

            item.ReleaseFromUndoContext();

            if (item is IEditCommittableItem tei)
            {
                tei.EditCommitted -= editableHandler;
            }

            this.AddFailure();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoReleaseTransactionContext" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="editableHandler">The editable handler.</param>
        public AutoReleaseTransactionContext(
            IEnumerable<IUndoableItem> items,
            IUndoableItem parentContext,
            EventHandler<EditCommittedEventArgs> editableHandler)
        {
            // Contract validation
            Contract.RequiresNotNullPrivate(
                in items,
                nameof(items));
            Contract.RequiresNotNullPrivate(
                in parentContext,
                nameof(parentContext));
            Contract.RequiresNotNullPrivate(
                in editableHandler,
                nameof(editableHandler));

            // Data validation
            // Multiple enumeration warning: this has to be done, as there is no efficient way to do a transactional capturing otherwise
            IUndoableItem[] itemsArray = items.ToArray();
            if (itemsArray.Any(
                (
                        item,
                        pc) => !item.IsCapturedIntoUndoContext || item.ParentUndoContext != pc, parentContext))
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            // State
            this.items = itemsArray;
            this.item = null;
            this.editableHandler = editableHandler;

            foreach (IUndoableItem undoableItem in itemsArray)
            {
                undoableItem.ReleaseFromUndoContext();

                if (undoableItem is IEditCommittableItem tei)
                {
                    tei.EditCommitted -= editableHandler;
                }
            }

            this.AddFailure();
        }

        /// <summary>
        ///     Gets invoked when the transaction commits and is successful.
        /// </summary>
        protected override void WhenSuccessful()
        {
        }

        private void AddFailure() => this.AddRevertStep(
            state =>
            {
                var thisL1 = (AutoReleaseTransactionContext)state;

                if (thisL1.item != null)
                {
                    thisL1.item.CaptureIntoUndoContext(thisL1.parentContext);

                    if (thisL1.item is IEditCommittableItem tei)
                    {
                        tei.EditCommitted += thisL1.editableHandler;
                    }
                }

                if (thisL1.items == null)
                {
                    return;
                }

                foreach (IUndoableItem undoableItem in thisL1.items)
                {
                    undoableItem.CaptureIntoUndoContext(thisL1.parentContext);

                    if (thisL1.item is IEditCommittableItem tei)
                    {
                        tei.EditCommitted += thisL1.editableHandler;
                    }
                }
            }, this);
    }
}