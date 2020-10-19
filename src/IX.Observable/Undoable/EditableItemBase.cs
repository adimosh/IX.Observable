// <copyright file="EditableItemBase.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using IX.StandardExtensions.ComponentModel;
using IX.StandardExtensions.Contracts;
using JetBrains.Annotations;

namespace IX.Undoable
{
    /// <summary>
    ///     A base class for editable items that can be edited in a transactional-style pattern.
    /// </summary>
    /// <seealso cref="IX.Undoable.ITransactionEditableItem" />
    /// <seealso cref="IX.Undoable.IUndoableItem" />
    [PublicAPI]
    public abstract class EditableItemBase : ViewModelBase, ITransactionEditableItem, IUndoableItem
    {
        private readonly List<StateChange> stateChanges;
        private int historyLevels;

        private Lazy<UndoableInnerContext> undoContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableItemBase" /> class.
        /// </summary>
        protected EditableItemBase()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableItemBase" /> class.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit" /> is a negative number.</exception>
        protected EditableItemBase(int limit)
        {
            Requires.NonNegative(
                out this.historyLevels,
                in limit,
                nameof(limit));

            this.undoContext = new Lazy<UndoableInnerContext>(this.InnerContextFactory);
            this.stateChanges = new List<StateChange>();
        }

        /// <summary>
        ///     Occurs when an edit on this item is committed.
        /// </summary>
        public event EventHandler<EditCommittedEventArgs> EditCommitted;

        /// <summary>
        ///     Gets a value indicating whether this instance is in edit mode.
        /// </summary>
        /// <value><see langword="true" /> if this instance is in edit mode; otherwise, <see langword="false" />.</value>
        public bool IsInEditMode { get; private set; }

        /// <summary>
        ///     Gets or sets the number of levels to keep undo or redo information.
        /// </summary>
        /// <value>The history levels.</value>
        /// <remarks>
        ///     <para>
        ///         If this value is set, for example, to 7, then the implementing object should allow the <see cref="Undo" />
        ///         method
        ///         to be called 7 times to change the state of the object. Upon calling it an 8th time, there should be no change
        ///         in the
        ///         state of the object.
        ///     </para>
        ///     <para>
        ///         Any call beyond the limit imposed here should not fail, but it should also not change the state of the
        ///         object.
        ///     </para>
        /// </remarks>
        public int HistoryLevels
        {
            get => this.historyLevels;
            set
            {
                if (value == this.historyLevels)
                {
                    return;
                }

                this.undoContext.Value.HistoryLevels = value;

                // We'll let the internal undo context to curate our history levels
                this.historyLevels = this.undoContext.Value.HistoryLevels;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is captured in undo context.
        /// </summary>
        /// <value><see langword="true" /> if this instance is captured in undo context; otherwise, <see langword="false" />.</value>
        public bool IsCapturedIntoUndoContext => this.ParentUndoContext != null;

        /// <summary>
        ///     Gets a value indicating whether or not an undo can be performed on this item.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the call to the <see cref="Undo" /> method would result in a state change,
        ///     <see langword="false" /> otherwise.
        /// </value>
        public bool CanUndo => this.IsCapturedIntoUndoContext ||
                               (this.undoContext.IsValueCreated && this.undoContext.Value.UndoStack.Count > 0);

        /// <summary>
        ///     Gets a value indicating whether or not a redo can be performed on this item.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the call to the <see cref="Redo" /> method would result in a state change,
        ///     <see langword="false" /> otherwise.
        /// </value>
        public bool CanRedo => this.IsCapturedIntoUndoContext ||
                               (this.undoContext.IsValueCreated && this.undoContext.Value.RedoStack.Count > 0);

        /// <summary>
        ///     Gets the parent undo context.
        /// </summary>
        /// <value>The parent undo context.</value>
        public IUndoableItem ParentUndoContext { get; private set; }

        /// <summary>
        ///     Begins the editing of an item.
        /// </summary>
        public void BeginEdit()
        {
            if (this.IsInEditMode)
            {
                return;
            }

            this.IsInEditMode = true;

            this.RaisePropertyChanged(nameof(this.IsInEditMode));
        }

        /// <summary>
        ///     Discards all changes to the item, reloading the state at the last commit or at the beginning of the edit
        ///     transaction, whichever occurred last.
        /// </summary>
        /// <exception cref="IX.Undoable.ItemNotInEditModeException">The item is not in edit mode.</exception>
        public void CancelEdit()
        {
            if (!this.IsInEditMode)
            {
                throw new ItemNotInEditModeException();
            }

            if (this.stateChanges.Count <= 0)
            {
                return;
            }

            this.RevertChanges(this.stateChanges.ToArray());

            this.stateChanges.Clear();
        }

        /// <summary>
        ///     Commits the changes to the item as they are, without ending the editing.
        /// </summary>
        /// <exception cref="IX.Undoable.ItemNotInEditModeException">The item is not in edit mode.</exception>
        public void CommitEdit()
        {
            if (!this.IsInEditMode)
            {
                throw new ItemNotInEditModeException();
            }

            if (this.stateChanges.Count <= 0)
            {
                return;
            }

            this.CommitEditInternal(this.stateChanges.ToArray());

            this.stateChanges.Clear();
        }

        /// <summary>
        ///     Ends the editing of an item.
        /// </summary>
        /// <exception cref="IX.Undoable.ItemNotInEditModeException">The item is not in edit mode.</exception>
        public void EndEdit()
        {
            if (!this.IsInEditMode)
            {
                throw new ItemNotInEditModeException();
            }

            if (this.stateChanges.Count > 0)
            {
                this.CommitEditInternal(this.stateChanges.ToArray());

                this.stateChanges.Clear();
            }

            this.IsInEditMode = false;

            this.RaisePropertyChanged(nameof(this.IsInEditMode));
        }

        /// <summary>
        ///     Allows the item to be captured by a containing undo-/redo-capable object so that undo and redo operations
        ///     can be coordinated across a larger scope.
        /// </summary>
        /// <param name="parent">The parent undo and redo context.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="parent" /> is <see langword="null" /> (
        ///     <see langword="Nothing" /> in Visual Basic).
        /// </exception>
        /// <exception cref="IX.Undoable.ItemIsInEditModeException">
        ///     The item is in edit mode, and this operation cannot be
        ///     performed at this time.
        /// </exception>
        /// <remarks>This method is meant to be used by containers, and should not be called directly.</remarks>
        public void CaptureIntoUndoContext(IUndoableItem parent)
        {
            Requires.NotNull(
                parent,
                nameof(parent));

            if (parent == this.ParentUndoContext)
            {
                return;
            }

            if (this.IsInEditMode)
            {
                throw new ItemIsInEditModeException();
            }

            // Set the parent undo context
            this.ParentUndoContext = parent;
            if (this.undoContext.IsValueCreated)
            {
                // We already have an undo inner context, let's clear it out.
                // If we don't have an undo inner context, no sense in clearing it out just yet.
                this.undoContext.Value.HistoryLevels = 0;
            }

            this.RaisePropertyChanged(nameof(this.IsCapturedIntoUndoContext));
        }

        /// <summary>
        ///     Releases the item from being captured into an undo and redo context.
        /// </summary>
        /// <remarks>This method is meant to be used by containers, and should not be called directly.</remarks>
        public void ReleaseFromUndoContext()
        {
            if (this.ParentUndoContext == null)
            {
                return;
            }

            // Set parent undo context as null
            this.ParentUndoContext = null;

            if (this.undoContext.IsValueCreated)
            {
                // We already have an undo inner context, let's set it back.
                // If we don't have an undo inner context, no sense in setting anything.
                this.undoContext.Value.HistoryLevels = this.historyLevels;
            }

            this.RaisePropertyChanged(nameof(this.IsCapturedIntoUndoContext));
        }

        /// <summary>
        ///     Has the last operation performed on the item undone.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the object is captured, the method will call the capturing parent's Undo method, which can bubble down to
        ///         the last instance of an undo-/redo-capable object.
        ///     </para>
        ///     <para>
        ///         If that is the case, the capturing object is solely responsible for ensuring that the inner state of the whole
        ///         system is correct. Implementing classes should not expect this method to also handle state.
        ///     </para>
        ///     <para>If the object is released, it is expected that this method once again starts ensuring state when called.</para>
        /// </remarks>
        public void Undo()
        {
            if (this.ParentUndoContext != null)
            {
                // We are captured by a parent context, let's invoke that context's Undo.
                this.ParentUndoContext.Undo();
                return;
            }

            // We are not captured, let's proceed with Undo.

            // Let's check whether or not we have an undo inner context first
            if (!this.undoContext.IsValueCreated)
            {
                // Undo inner context not created, there's nothing to undo
                return;
            }

            // Undo context created, let's try to undo
            UndoableInnerContext uc = this.undoContext.Value;
            if (uc.UndoStack.Count == 0)
            {
                // We don't have anything to Undo.
                return;
            }

            StateChange[] undoData = uc.UndoStack.Pop();
            uc.RedoStack.Push(undoData);
            this.RevertChanges(undoData);

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        ///     Has the last undone operation performed on this item, presuming that it has not changed since then, redone.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the object is captured, the method will call the capturing parent's Redo method, which can bubble down to
        ///         the last instance of an undo-/redo-capable object.
        ///     </para>
        ///     <para>
        ///         If that is the case, the capturing object is solely responsible for ensuring that the inner state of the whole
        ///         system is correct. Implementing classes should not expect this method to also handle state.
        ///     </para>
        ///     <para>If the object is released, it is expected that this method once again starts ensuring state when called.</para>
        /// </remarks>
        public void Redo()
        {
            if (this.ParentUndoContext != null)
            {
                // We are captured by a parent context, let's invoke that context's Redo.
                this.ParentUndoContext.Redo();
                return;
            }

            // We are not captured, let's proceed with Undo.

            // Let's check whether or not we have an undo inner context first
            if (!this.undoContext.IsValueCreated)
            {
                // Undo inner context not created, there's nothing to undo
                return;
            }

            // Undo context created, let's try to undo
            UndoableInnerContext uc = this.undoContext.Value;
            if (uc.RedoStack.Count == 0)
            {
                // We don't have anything to Redo.
                return;
            }

            StateChange[] redoData = uc.RedoStack.Pop();
            uc.UndoStack.Push(redoData);
            this.DoChanges(redoData);

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        ///     Has the state changes received undone from the object.
        /// </summary>
        /// <param name="changesToUndo">The state changes to undo.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="changesToUndo" /> is <see langword="null" /> (
        ///     <see langword="Nothing" /> in Visual Basic).
        /// </exception>
        /// <exception cref="ItemNotCapturedIntoUndoContextException">
        ///     The item is not captured into an undo/redo context, and this
        ///     operation is illegal.
        /// </exception>
        public void UndoStateChanges(StateChange[] changesToUndo)
        {
            Requires.NotNull(
                changesToUndo,
                nameof(changesToUndo));

            if (!this.IsCapturedIntoUndoContext)
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            if (changesToUndo.Length > 0)
            {
                this.RevertChanges(changesToUndo);
            }
        }

        /// <summary>
        ///     Has the state changes received redone into the object.
        /// </summary>
        /// <param name="changesToRedo">The state changes to redo.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="changesToRedo" /> is <see langword="null" /> (
        ///     <see langword="Nothing" /> in Visual Basic).
        /// </exception>
        /// <exception cref="ItemNotCapturedIntoUndoContextException">
        ///     The item is not captured into an undo/redo context, and this
        ///     operation is illegal.
        /// </exception>
        public void RedoStateChanges(StateChange[] changesToRedo)
        {
            Requires.NotNull(
                changesToRedo,
                nameof(changesToRedo));

            if (!this.IsCapturedIntoUndoContext)
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            if (changesToRedo.Length > 0)
            {
                this.DoChanges(changesToRedo);
            }
        }

        /// <summary>
        ///     Captures a sub item into the present context.
        /// </summary>
        /// <typeparam name="TSubItem">The type of the sub item.</typeparam>
        /// <param name="item">The item.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="item" />
        ///     is <see langword="null" /> (<see langword="Nothing" /> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///     This method is intended to capture only objects that are directly sub-objects that can have their own internal
        ///     state and undo/redo
        ///     capabilities and are also transactional in nature when being edited. Using this method on any other object may
        ///     yield unwanted
        ///     commits.
        /// </remarks>
        protected void CaptureSubItemIntoPresentContext<TSubItem>(TSubItem item)
            where TSubItem : IUndoableItem, IEditCommittableItem
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.CaptureIntoUndoContext(this);

            item.EditCommitted += this.Item_EditCommitted;
        }

        /// <summary>
        ///     Releases the sub item from present context.
        /// </summary>
        /// <typeparam name="TSubItem">The type of the sub item.</typeparam>
        /// <param name="item">The item.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="item" />
        ///     is <see langword="null" /> (<see langword="Nothing" /> in Visual Basic).
        /// </exception>
        protected void ReleaseSubItemFromPresentContext<TSubItem>(TSubItem item)
            where TSubItem : IUndoableItem, IEditCommittableItem
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.EditCommitted -= this.Item_EditCommitted;

            item.ReleaseFromUndoContext();
        }

        /// <summary>
        ///     Can be called to advertise a change of state in an implementing class.
        /// </summary>
        /// <param name="stateChange">The state change to advertise.</param>
        protected void AdvertiseStateChange(StateChange stateChange)
        {
            if (this.IsInEditMode)
            {
                this.stateChanges.Add(stateChange);
            }
            else
            {
                this.CommitEditInternal(new[] { stateChange });
            }
        }

        /// <summary>
        ///     Advertises a property change.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected void AdvertisePropertyChange<T>(
            string propertyName,
            T oldValue,
            T newValue) => this.AdvertiseStateChange(
            new PropertyStateChange<T>
            {
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            });

        /// <summary>
        ///     Disposes in the managed context.
        /// </summary>
        protected override void DisposeManagedContext()
        {
            base.DisposeManagedContext();

            var uc = Interlocked.Exchange(
                ref this.undoContext,
                null);

            if (uc?.IsValueCreated ?? false)
            {
                uc.Value.Dispose();
            }
        }

        /// <summary>
        ///     Called when a list of state changes are canceled and must be reverted.
        /// </summary>
        /// <param name="stateChanges">The state changes to revert.</param>
        protected abstract void RevertChanges(StateChange[] stateChanges);

        /// <summary>
        ///     Called when a list of state changes must be executed.
        /// </summary>
        /// <param name="stateChanges">The state changes to execute.</param>
        protected abstract void DoChanges(StateChange[] stateChanges);

        /// <summary>
        ///     Handles the EditCommitted event of the sub-item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditCommittedEventArgs" /> instance containing the event data.</param>
        private void Item_EditCommitted(
            object sender,
            EditCommittedEventArgs e)
        {
            this.stateChanges.Add(
                new SubItemStateChange { StateChanges = e.StateChanges, SubObject = (IUndoableItem)sender });

            this.CommitEditInternal(this.stateChanges.ToArray());
        }

        /// <summary>
        ///     Commits the edit internally.
        /// </summary>
        /// <param name="changesToCommit">The state changes.</param>
        private void CommitEditInternal(StateChange[] changesToCommit)
        {
            if ((changesToCommit?.Length ?? 0) == 0)
            {
                return;
            }

            if (this.ParentUndoContext == null && this.historyLevels > 0)
            {
                this.undoContext.Value.UndoStack.Push(changesToCommit);
                this.undoContext.Value.RedoStack.Clear();
            }

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));

            this.EditCommitted?.Invoke(
                this,
                new EditCommittedEventArgs(changesToCommit));
        }

        private UndoableInnerContext InnerContextFactory() => new UndoableInnerContext
        {
            HistoryLevels = this.historyLevels
        };
    }
}