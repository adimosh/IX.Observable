// <copyright file="ObservableCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using IX.Guaranteed;
using IX.Observable.Adapters;
using IX.Observable.UndoLevels;
using IX.StandardExtensions.Contracts;
using IX.StandardExtensions.Threading;
using IX.Undoable;
using JetBrains.Annotations;

namespace IX.Observable
{
    /// <summary>
    ///     A base class for collections that are observable.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="INotifyCollectionChanged" />
    /// <seealso cref="IEnumerable{T}" />
    [PublicAPI]
    public abstract class ObservableCollectionBase<T> : ObservableReadOnlyCollectionBase<T>, ICollection<T>,
        IUndoableItem, IEditCommittableItem
    {
        private bool automaticallyCaptureSubItems;
        private UndoableUnitBlockTransaction<T> currentUndoBlockTransaction;
        private int historyLevels;
        private bool suppressUndoable;
        private Lazy<UndoableInnerContext> undoContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollectionBase{T}" /> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        protected ObservableCollectionBase(ICollectionAdapter<T> internalContainer)
            : base(internalContainer)
        {
            this.InitializeInternalState(internalContainer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollectionBase{T}" /> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableCollectionBase(
            ICollectionAdapter<T> internalContainer,
            SynchronizationContext context)
            : base(
                internalContainer,
                context)
        {
            this.InitializeInternalState(internalContainer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollectionBase{T}" /> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        protected ObservableCollectionBase(
            ICollectionAdapter<T> internalContainer,
            bool suppressUndoable)
            : base(internalContainer)
        {
            this.InitializeInternalState(
                internalContainer,
                suppressUndoable);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollectionBase{T}" /> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        protected ObservableCollectionBase(
            ICollectionAdapter<T> internalContainer,
            SynchronizationContext context,
            bool suppressUndoable)
            : base(
                internalContainer,
                context)
        {
            this.InitializeInternalState(
                internalContainer,
                suppressUndoable);
        }

        /// <summary>
        ///     Occurs when an edit is committed to the collection, whichever that may be.
        /// </summary>
        protected event EventHandler<EditCommittedEventArgs> EditCommittedInternal;

        /// <summary>
        ///     Occurs when an edit on this item is committed.
        /// </summary>
        /// <remarks>
        ///     <para>Warning! This event is invoked within the write lock on concurrent collections.</para>
        /// </remarks>
        event EventHandler<EditCommittedEventArgs> IEditCommittableItem.EditCommitted
        {
            add => this.EditCommittedInternal += value;
            remove => this.EditCommittedInternal -= value;
        }

        /// <summary>
        ///     Gets a value indicating whether or not the implementer can perform a redo.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the call to the <see cref="M:IX.Undoable.IUndoableItem.Redo" /> method would result
        ///     in a state change, <see langword="false" /> otherwise.
        /// </value>
        public bool CanRedo
        {
            get
            {
                if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
                {
                    return false;
                }

                this.RequiresNotDisposed();

                return this.ParentUndoContext?.CanRedo ?? this.ReadLock(
                           thisL1 => thisL1.undoContext.IsValueCreated && thisL1.undoContext.Value.RedoStack.Count > 0,
                           this);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not the implementer can perform an undo.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the call to the <see cref="M:IX.Undoable.IUndoableItem.Undo" /> method would result
        ///     in a state change, <see langword="false" /> otherwise.
        /// </value>
        public bool CanUndo
        {
            get
            {
                if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
                {
                    return false;
                }

                this.RequiresNotDisposed();

                return this.ParentUndoContext?.CanUndo ?? this.ReadLock(
                           thisL1 => thisL1.undoContext.IsValueCreated && thisL1.undoContext.Value.UndoStack.Count > 0,
                           this);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is caught into an undo context.
        /// </summary>
        public bool IsCapturedIntoUndoContext => this.ParentUndoContext != null;

        /// <summary>
        ///     Gets a value indicating whether items are key/value pairs.
        /// </summary>
        /// <value><see langword="true" /> if items are key/value pairs; otherwise, <see langword="false" />.</value>
        public bool ItemsAreKeyValuePairs { get; }

        /// <summary>
        ///     Gets a value indicating whether items are undoable.
        /// </summary>
        /// <value><see langword="true" /> if items are undoable; otherwise, <see langword="false" />.</value>
        public bool ItemsAreUndoable { get; } =
            typeof(IUndoableItem).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());

        /// <summary>
        ///     Gets or sets a value indicating whether to automatically capture sub items in the current undo/redo context.
        /// </summary>
        /// <value><see langword="true" /> to automatically capture sub items; otherwise, <see langword="false" />.</value>
        /// <remarks>
        ///     <para>This property does nothing if the items of the observable collection are not undoable themselves.</para>
        ///     <para>
        ///         To check whether or not the items are undoable at runtime, please use the <see cref="ItemsAreUndoable" />
        ///         property.
        ///     </para>
        /// </remarks>
        public bool AutomaticallyCaptureSubItems
        {
            get => this.automaticallyCaptureSubItems;

            set => this.SetAutomaticallyCaptureSubItems(
                value,
                false);
        }

        /// <summary>
        ///     Gets or sets the number of levels to keep undo or redo information.
        /// </summary>
        /// <value>The history levels.</value>
        /// <remarks>
        ///     <para>
        ///         If this value is set, for example, to 7, then the implementing object should allow the
        ///         <see cref="M:IX.Undoable.IUndoableItem.Undo" /> method
        ///         to be called 7 times to change the state of the object. Upon calling it an 8th time, there should be no change
        ///         in the
        ///         state of the object.
        ///     </para>
        ///     <para>
        ///         Any call beyond the limit imposed here should not fail, but it should also not change the state of the
        ///         object.
        ///     </para>
        ///     <para>
        ///         This member is not serialized, as it interferes with the undo/redo context, which cannot itself be
        ///         serialized.
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
        ///     Gets the parent undo context, if any.
        /// </summary>
        /// <value>The parent undo context.</value>
        /// <remarks>
        ///     <para>This member is not serialized, as it represents the undo/redo context, which cannot itself be serialized.</para>
        ///     <para>
        ///         The concept of the undo/redo context is incompatible with serialization. Any collection that is serialized will
        ///         be free of any original context
        ///         when deserialized.
        ///     </para>
        /// </remarks>
        public IUndoableItem ParentUndoContext { get; private set; }

        /// <summary>
        ///     Adds an item to the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <remarks>
        ///     <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public virtual void Add(T item)
        {
            // PRECONDITIONS

            // Current object not disposed
            this.RequiresNotDisposed();

            // ACTION
            int newIndex;

            // Under write lock
            using (this.WriteLock())
            {
                // Using an undo/redo transaction lock
                using (OperationTransaction tc = this.CheckItemAutoCapture(item))
                {
                    // Add the item
                    newIndex = this.InternalContainer.Add(item);

                    // Push the undo level
                    this.PushUndoLevel(new AddUndoLevel<T> { AddedItem = item, Index = newIndex });

                    // Mark the transaction as a success
                    tc.Success();
                }
            }

            // NOTIFICATIONS

            // Collection changed
            if (newIndex == -1)
            {
                // If no index could be found for an item (Dictionary add)
                this.RaiseCollectionReset();
            }
            else
            {
                // If index was added at a specific index
                this.RaiseCollectionChangedAdd(
                    item,
                    newIndex);
            }

            // Property changed
            this.RaisePropertyChanged(nameof(this.Count));

            // Contents may have changed
            this.ContentsMayHaveChanged();
        }

        /// <summary>
        ///     Removes all items from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <remarks>
        ///     <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public void Clear() => this.ClearInternal();

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> was successfully removed from the
        ///     <see cref="ObservableCollectionBase{T}" />; otherwise, <see langword="false" />.
        ///     This method also returns false if <paramref name="item" /> is not found in the original
        ///     <see cref="ObservableCollectionBase{T}" />.
        /// </returns>
        /// <remarks>
        ///     <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public virtual bool Remove(T item)
        {
            // PRECONDITIONS

            // Current object not disposed
            this.RequiresNotDisposed();

            // ACTION
            int oldIndex;

            // Under write lock
            using (this.WriteLock())
            {
                // Inside an undo/redo transaction
                using (OperationTransaction tc = this.CheckItemAutoRelease(item))
                {
                    // Remove the item
                    oldIndex = this.InternalContainer.Remove(item);

                    // Push an undo level
                    this.PushUndoLevel(new RemoveUndoLevel<T> { RemovedItem = item, Index = oldIndex });

                    // Mark the transaction as a success
                    tc.Success();
                }
            }

            // NOTIFICATIONS AND RETURN

            // Collection changed
            if (oldIndex >= 0)
            {
                // Collection changed with a specific index
                this.RaiseCollectionChangedRemove(
                    item,
                    oldIndex);

                // Property changed
                this.RaisePropertyChanged(nameof(this.Count));

                // Contents may have changed
                this.ContentsMayHaveChanged();

                return true;
            }

            if (oldIndex >= -1)
            {
                // Unsuccessful removal
                return false;
            }

            // Collection changed with no specific index (Dictionary remove)
            this.RaiseCollectionReset();

            // Property changed
            this.RaisePropertyChanged(nameof(this.Count));

            // Contents may have changed
            this.ContentsMayHaveChanged();

            return true;
        }

        /// <summary>
        ///     Allows the implementer to be captured by a containing undo-/redo-capable object so that undo and redo operations
        ///     can be coordinated across a larger scope.
        /// </summary>
        /// <param name="parent">The parent undo and redo context.</param>
        public void CaptureIntoUndoContext(IUndoableItem parent) => this.CaptureIntoUndoContext(
            parent,
            false);

        /// <summary>
        ///     Releases the implementer from being captured into an undo and redo context.
        /// </summary>
        public void ReleaseFromUndoContext()
        {
            this.RequiresNotDisposed();

            using (this.WriteLock())
            {
                this.SetAutomaticallyCaptureSubItems(
                    false,
                    true);
                this.ParentUndoContext = null;
            }
        }

        /// <summary>
        ///     Has the last operation performed on the implementing instance undone.
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
            if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
            {
                return;
            }

            this.RequiresNotDisposed();

            if (this.ParentUndoContext != null)
            {
                this.ParentUndoContext.Undo();
                return;
            }

            if (this.currentUndoBlockTransaction != null)
            {
                throw new InvalidOperationException(
                    Resources.UndoAndRedoOperationsAreNotSupportedWhileAnExplicitTransactionBlockIsOpen);
            }

            Action<object> toInvoke;
            object state;
            bool internalResult;
            using (ReadWriteSynchronizationLocker locker = this.ReadWriteLock())
            {
                if (!this.undoContext.IsValueCreated || this.undoContext.Value.UndoStack.Count == 0)
                {
                    return;
                }

                locker.Upgrade();

                UndoableInnerContext uc = this.undoContext.Value;

                StateChange[] level = uc.UndoStack.Pop();
                internalResult = this.UndoInternally(
                    level[0],
                    out toInvoke,
                    out state);
                if (internalResult)
                {
                    uc.RedoStack.Push(level);
                }
            }

            if (internalResult)
            {
                toInvoke?.Invoke(state);
            }

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        ///     Has the last undone operation performed on the implemented instance, presuming that it has not changed, redone.
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
            if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
            {
                return;
            }

            this.RequiresNotDisposed();

            if (this.ParentUndoContext != null)
            {
                this.ParentUndoContext.Redo();
                return;
            }

            if (this.currentUndoBlockTransaction != null)
            {
                throw new InvalidOperationException(
                    Resources.UndoAndRedoOperationsAreNotSupportedWhileAnExplicitTransactionBlockIsOpen);
            }

            Action<object> toInvoke;
            object state;
            bool internalResult;
            using (ReadWriteSynchronizationLocker locker = this.ReadWriteLock())
            {
                if (!this.undoContext.IsValueCreated || this.undoContext.Value.RedoStack.Count == 0)
                {
                    return;
                }

                locker.Upgrade();

                UndoableInnerContext uc = this.undoContext.Value;

                StateChange[] level = uc.RedoStack.Pop();
                internalResult = this.RedoInternally(
                    level[0],
                    out toInvoke,
                    out state);
                if (internalResult)
                {
                    uc.UndoStack.Push(level);
                }
            }

            if (internalResult)
            {
                toInvoke?.Invoke(state);
            }

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        ///     Has the state changes received undone from the object.
        /// </summary>
        /// <param name="changesToUndo">The state changes to redo.</param>
        /// <exception cref="ItemNotCapturedIntoUndoContextException">There is no capturing context.</exception>
        public void UndoStateChanges(StateChange[] changesToUndo)
        {
            // PRECONDITIONS
            if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
            {
                return;
            }

            // Current object not disposed
            this.RequiresNotDisposed();

            // Current object captured in an undo/redo context
            if (!this.IsCapturedIntoUndoContext)
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            // ACTION
            foreach (StateChange sc in changesToUndo)
            {
                switch (sc)
                {
                    case SubItemStateChange sisc:
                    {
                        sisc.SubObject.UndoStateChanges(sisc.StateChanges);

                        break;
                    }

                    case BlockStateChange bsc:
                    {
                        foreach (StateChange ulsc in bsc.StateChanges)
                        {
                            Action<object> act;
                            object state;
                            bool internalResult;
                            using (this.WriteLock())
                            {
                                internalResult = this.UndoInternally(
                                    ulsc,
                                    out act,
                                    out state);
                            }

                            if (internalResult)
                            {
                                act?.Invoke(state);
                            }
                        }

                        break;
                    }

                    case StateChange ulsc:
                    {
                        Action<object> act;
                        object state;
                        bool internalResult;

                        using (this.WriteLock())
                        {
                            internalResult = this.UndoInternally(
                                ulsc,
                                out act,
                                out state);
                        }

                        if (internalResult)
                        {
                            act?.Invoke(state);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Has the state changes received redone into the object.
        /// </summary>
        /// <param name="changesToRedo">The state changes to redo.</param>
        /// <exception cref="ItemNotCapturedIntoUndoContextException">There is no capturing context.</exception>
        public void RedoStateChanges(StateChange[] changesToRedo)
        {
            // PRECONDITIONS
            if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
            {
                return;
            }

            // Current object not disposed
            this.RequiresNotDisposed();

            // Current object captured in an undo/redo context
            if (!this.IsCapturedIntoUndoContext)
            {
                throw new ItemNotCapturedIntoUndoContextException();
            }

            // ACTION
            foreach (StateChange sc in changesToRedo)
            {
                switch (sc)
                {
                    case SubItemStateChange sisc:
                    {
                        sisc.SubObject.RedoStateChanges(sisc.StateChanges);

                        break;
                    }

                    case BlockStateChange blsc:
                    {
                        foreach (StateChange ulsc in blsc.StateChanges)
                        {
                            object state;
                            bool internalResult;
                            Action<object> act;
                            using (this.WriteLock())
                            {
                                internalResult = this.RedoInternally(
                                    ulsc,
                                    out act,
                                    out state);
                            }

                            if (internalResult)
                            {
                                act?.Invoke(state);
                            }
                        }

                        break;
                    }

                    case StateChange ulsc:
                    {
                        Action<object> act;
                        object state;
                        bool internalResult;

                        using (this.WriteLock())
                        {
                            internalResult = this.RedoInternally(
                                ulsc,
                                out act,
                                out state);
                        }

                        if (internalResult)
                        {
                            act?.Invoke(state);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Starts the undoable operations on this object.
        /// </summary>
        /// <remarks>
        ///     <para>If undoable operations were suppressed, no undo levels will accumulate before calling this method.</para>
        /// </remarks>
        public void StartUndo() => this.suppressUndoable = false;

        /// <summary>
        ///     Removes all items from the <see cref="ObservableCollectionBase{T}" /> and returns them as an array.
        /// </summary>
        /// <returns>An array containing the original collection items.</returns>
        /// <remarks>
        ///     <para>On concurrent collections, this method is write-synchronized.</para>
        /// </remarks>
        public T[] ClearAndPersist() => this.ClearInternal();

        /// <summary>
        ///     Allows the implementer to be captured by a containing undo-/redo-capable object so that undo and redo operations
        ///     can be coordinated across a larger scope.
        /// </summary>
        /// <param name="parent">The parent undo and redo context.</param>
        /// <param name="captureSubItems">
        ///     if set to <see langword="true" />, the collection automatically captures sub-items into
        ///     its undo/redo context.
        /// </param>
        public void CaptureIntoUndoContext(
            IUndoableItem parent,
            bool captureSubItems)
        {
            this.RequiresNotDisposed();

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            using (this.WriteLock())
            {
                this.SetAutomaticallyCaptureSubItems(
                    captureSubItems,
                    true);
                this.ParentUndoContext = parent;
            }
        }

        /// <summary>
        ///     Starts an explicit undo block transaction.
        /// </summary>
        /// <returns>OperationTransaction.</returns>
        public OperationTransaction StartExplicitUndoBlockTransaction()
        {
            if (this.IsCapturedIntoUndoContext)
            {
                throw new InvalidOperationException(
                    Resources.TheCollectionIsCapturedIntoAContextItCannotStartAnExplicitTransaction);
            }

            if (this.currentUndoBlockTransaction != null)
            {
                throw new InvalidOperationException(Resources.ThereAlreadyIsAnOpenUndoTransaction);
            }

            var transaction = new UndoableUnitBlockTransaction<T>(this);

            Interlocked.Exchange(
                ref this.currentUndoBlockTransaction,
                transaction);

            return transaction;
        }

        /// <summary>
        ///     Finishes the explicit undo block transaction.
        /// </summary>
        internal void FinishExplicitUndoBlockTransaction()
        {
            this.undoContext.Value.UndoStack.Push(new StateChange[] { this.currentUndoBlockTransaction.StateChanges });

            Interlocked.Exchange(
                ref this.currentUndoBlockTransaction,
                null);

            this.undoContext.Value.RedoStack.Clear();

            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        ///     Fails the explicit undo block transaction.
        /// </summary>
        internal void FailExplicitUndoBlockTransaction() => Interlocked.Exchange(
            ref this.currentUndoBlockTransaction,
            null);

        /// <summary>
        ///     Disposes the managed context.
        /// </summary>
        protected override void DisposeManagedContext()
        {
            base.DisposeManagedContext();

            Lazy<UndoableInnerContext> uc = Interlocked.Exchange(
                ref this.undoContext,
                null);

            if (uc?.IsValueCreated ?? false)
            {
                uc.Value.Dispose();
            }
        }

#pragma warning disable HAA0401 // Possible allocation of reference type enumerator - OK, we're doing a Reverse anyway
        /// <summary>
        ///     Has the last operation undone.
        /// </summary>
        /// <param name="undoRedoLevel">A level of undo, with contents.</param>
        /// <param name="toInvokeOutsideLock">An action to invoke outside of the lock.</param>
        /// <param name="state">The state object to pass to the invocation.</param>
        /// <returns><see langword="true" /> if the undo was successful, <see langword="false" /> otherwise.</returns>
        protected virtual bool UndoInternally(
            [NotNull] StateChange undoRedoLevel,
            [CanBeNull] out Action<object> toInvokeOutsideLock,
            [CanBeNull] out object state)
        {
            if (undoRedoLevel is SubItemStateChange lvl)
            {
                lvl.SubObject.UndoStateChanges(lvl.StateChanges);

                toInvokeOutsideLock = null;
                state = null;

                return true;
            }

            if (undoRedoLevel is BlockStateChange bsc)
            {
                var count = bsc.StateChanges.Count;
                if (count == 0)
                {
                    toInvokeOutsideLock = null;
                    state = null;

                    return true;
                }

                var actionsToInvoke = new Action<object>[count];
                var states = new object[count];
                var counter = 0;
                var result = true;
                foreach (StateChange sc in ((IEnumerable<StateChange>)bsc.StateChanges).Reverse())
                {
                    try
                    {
                        var localResult = this.UndoInternally(
                            sc,
                            out Action<object> toInvoke,
                            out object toState);

                        if (!localResult)
                        {
                            result = false;
                        }
                        else
                        {
                            actionsToInvoke[counter] = toInvoke;
                            states[counter] = toState;
                        }
                    }
                    finally
                    {
                        counter++;
                    }
                }

                state = new Tuple<Action<object>[], object[], ObservableCollectionBase<T>>(
                    actionsToInvoke,
                    states,
                    this);
                toInvokeOutsideLock = innerState =>
                {
                    var convertedState = (Tuple<Action<object>[], object[], ObservableCollectionBase<T>>)innerState;

                    convertedState.Item3.InterpretBlockStateChangesOutsideLock(
                        convertedState.Item1,
                        convertedState.Item2);
                };

                return result;
            }

            toInvokeOutsideLock = null;
            state = null;

            return false;
        }
#pragma warning restore HAA0401 // Possible allocation of reference type enumerator

        /// <summary>
        ///     Has the last undone operation redone.
        /// </summary>
        /// <param name="undoRedoLevel">A level of undo, with contents.</param>
        /// <param name="toInvokeOutsideLock">An action to invoke outside of the lock.</param>
        /// <param name="state">The state object to pass to the invocation.</param>
        /// <returns><see langword="true" /> if the redo was successful, <see langword="false" /> otherwise.</returns>
        protected virtual bool RedoInternally(
            [NotNull] StateChange undoRedoLevel,
            [CanBeNull] out Action<object> toInvokeOutsideLock,
            [CanBeNull] out object state)
        {
            if (undoRedoLevel is SubItemStateChange lvl)
            {
                lvl.SubObject.RedoStateChanges(lvl.StateChanges);

                toInvokeOutsideLock = null;
                state = null;

                return true;
            }

            if (undoRedoLevel is BlockStateChange bsc)
            {
                var count = bsc.StateChanges.Count;
                if (count == 0)
                {
                    toInvokeOutsideLock = null;
                    state = null;

                    return true;
                }

                var actionsToInvoke = new Action<object>[count];
                var states = new object[count];
                var counter = 0;
                var result = true;
                foreach (StateChange sc in bsc.StateChanges)
                {
                    try
                    {
                        var localResult = this.RedoInternally(
                            sc,
                            out Action<object> toInvoke,
                            out object toState);

                        if (!localResult)
                        {
                            result = false;
                        }
                        else
                        {
                            actionsToInvoke[counter] = toInvoke;
                            states[counter] = toState;
                        }
                    }
                    finally
                    {
                        counter++;
                    }
                }

                state = new Tuple<Action<object>[], object[], ObservableCollectionBase<T>>(
                    actionsToInvoke,
                    states,
                    this);
                toInvokeOutsideLock = innerState =>
                {
                    var convertedState = (Tuple<Action<object>[], object[], ObservableCollectionBase<T>>)innerState;

                    convertedState.Item3.InterpretBlockStateChangesOutsideLock(
                        convertedState.Item1,
                        convertedState.Item2);
                };

                return result;
            }

            toInvokeOutsideLock = null;
            state = null;

            return false;
        }

        /// <summary>
        ///     Interprets the block state changes outside the write lock.
        /// </summary>
        /// <param name="actions">The actions to employ.</param>
        /// <param name="states">The state objects to send to the corresponding actions.</param>
        protected virtual void InterpretBlockStateChangesOutsideLock(
            Action<object>[] actions,
            object[] states)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                actions[i]?.Invoke(states[i]);
            }
        }

        /// <summary>
        ///     Push an undo level into the stack.
        /// </summary>
        /// <param name="undoRedoLevel">The undo level to push.</param>
        protected void PushUndoLevel(StateChange undoRedoLevel)
        {
            if (this.suppressUndoable || EnvironmentSettings.DisableUndoable || this.historyLevels == 0)
            {
                return;
            }

            if (this.IsCapturedIntoUndoContext)
            {
                this.EditCommittedInternal?.Invoke(
                    this,
                    new EditCommittedEventArgs(undoRedoLevel));

                this.undoContext.Value.RedoStack.Clear();

                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
            }
            else if (this.currentUndoBlockTransaction == null)
            {
                this.undoContext.Value.UndoStack.Push(new[] { undoRedoLevel });

                this.undoContext.Value.RedoStack.Clear();

                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
            }
            else
            {
                this.currentUndoBlockTransaction.StateChanges.StateChanges.Add(undoRedoLevel);
            }
        }

        /// <summary>
        ///     Called when an item is added to a collection.
        /// </summary>
        /// <param name="addedItem">The added item.</param>
        /// <param name="index">The index.</param>
        protected virtual void RaiseCollectionChangedAdd(
            T addedItem,
            int index) => this.RaiseCollectionAdd(
            index,
            addedItem);

        /// <summary>
        ///     Called when an item in a collection is changed.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="index">The index.</param>
        protected virtual void RaiseCollectionChangedChanged(
            T oldItem,
            T newItem,
            int index) => this.RaiseCollectionReplace(
            index,
            oldItem,
            newItem);

        /// <summary>
        ///     Called when an item is removed from a collection.
        /// </summary>
        /// <param name="removedItem">The removed item.</param>
        /// <param name="index">The index.</param>
        protected virtual void RaiseCollectionChangedRemove(
            T removedItem,
            int index) => this.RaiseCollectionRemove(
            index,
            removedItem);

        /// <summary>
        ///     Checks and automatically captures an item in a capturing transaction.
        /// </summary>
        /// <param name="item">The item to capture.</param>
        /// <returns>An auto-capture transaction context that reverts the capture if things go wrong.</returns>
        protected virtual OperationTransaction CheckItemAutoCapture(T item)
        {
            if (this.AutomaticallyCaptureSubItems && this.ItemsAreUndoable)
            {
                if (item is IUndoableItem ui)
                {
                    return new AutoCaptureTransactionContext(
                        ui,
                        this,
                        this.Tei_EditCommitted);
                }
            }

            return new AutoCaptureTransactionContext();
        }

        /// <summary>
        ///     Checks and automatically captures items in a capturing transaction.
        /// </summary>
        /// <param name="items">The items to capture.</param>
        /// <returns>An auto-capture transaction context that reverts the capture if things go wrong.</returns>
        protected virtual OperationTransaction CheckItemAutoCapture(IEnumerable<T> items)
        {
            if (this.AutomaticallyCaptureSubItems && this.ItemsAreUndoable)
            {
                return new AutoCaptureTransactionContext(
                    items.Cast<IUndoableItem>(),
                    this,
                    this.Tei_EditCommitted);
            }

            return new AutoCaptureTransactionContext();
        }

        /// <summary>
        ///     Checks and automatically captures an item in a capturing transaction.
        /// </summary>
        /// <param name="item">The item to capture.</param>
        /// <returns>An auto-capture transaction context that reverts the capture if things go wrong.</returns>
        protected virtual OperationTransaction CheckItemAutoRelease(T item)
        {
            if (this.AutomaticallyCaptureSubItems && this.ItemsAreUndoable)
            {
                if (item is IUndoableItem ui)
                {
                    return new AutoReleaseTransactionContext(
                        ui,
                        this,
                        this.Tei_EditCommitted);
                }
            }

            return new AutoReleaseTransactionContext();
        }

        /// <summary>
        ///     Checks and automatically captures items in a capturing transaction.
        /// </summary>
        /// <param name="items">The items to capture.</param>
        /// <returns>An auto-capture transaction context that reverts the capture if things go wrong.</returns>
        protected virtual OperationTransaction CheckItemAutoRelease(IEnumerable<T> items)
        {
            if (this.AutomaticallyCaptureSubItems && this.ItemsAreUndoable)
            {
                return new AutoReleaseTransactionContext(
                    items.Cast<IUndoableItem>(),
                    this,
                    this.Tei_EditCommitted);
            }

            return new AutoReleaseTransactionContext();
        }

        /// <summary>
        ///     Removes all items from the <see cref="ObservableCollectionBase{T}" /> and returns them as an array.
        /// </summary>
        /// <returns>An array containing the original collection items.</returns>
        protected virtual T[] ClearInternal()
        {
            // PRECONDITIONS

            // Current object not disposed
            this.RequiresNotDisposed();

            // ACTION
            T[] tempArray;

            // Under write lock
            using (this.WriteLock())
            {
                // Save existing items
                tempArray = new T[((ICollection<T>)this.InternalContainer).Count];
                this.InternalContainer.CopyTo(
                    tempArray,
                    0);

                // Into an undo/redo transaction context
                using (OperationTransaction tc = this.CheckItemAutoRelease(tempArray))
                {
                    // Do the actual clearing
                    this.InternalContainer.Clear();

                    // Push an undo level
                    this.PushUndoLevel(new ClearUndoLevel<T> { OriginalItems = tempArray });

                    // Mark the transaction as a success
                    tc.Success();
                }
            }

            // NOTIFICATIONS

            // Collection changed
            this.RaiseCollectionReset();

            // Property changed
            this.RaisePropertyChanged(nameof(this.Count));

            // Contents may have changed
            this.ContentsMayHaveChanged();

            return tempArray;
        }

        private void Tei_EditCommitted(
            object sender,
            EditCommittedEventArgs e) => this.PushUndoLevel(
            new SubItemStateChange { SubObject = sender as IUndoableItem, StateChanges = e.StateChanges });

        private void InitializeInternalState(
            ICollectionAdapter<T> internalContainer,
            bool? currentlySuppressUndoable = null)
        {
            this.InternalContainer = internalContainer;
            this.suppressUndoable = EnvironmentSettings.DisableUndoable ||
                                    (currentlySuppressUndoable ??
                                     EnvironmentSettings.AlwaysSuppressUndoLevelsByDefault);
            this.undoContext = new Lazy<UndoableInnerContext>(this.InnerContextFactory);
            this.historyLevels = EnvironmentSettings.DefaultUndoRedoLevels;
        }

        private UndoableInnerContext InnerContextFactory() => new UndoableInnerContext
        {
            HistoryLevels = this.historyLevels
        };

#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
        private void SetAutomaticallyCaptureSubItems(bool value, bool lockAcquired)
        {
            this.automaticallyCaptureSubItems = value;

            if (!this.ItemsAreUndoable)
            {
                return;
            }

            ReadWriteSynchronizationLocker locker = lockAcquired ? null : this.ReadWriteLock();

            if (value)
            {
                // At this point we start capturing
                try
                {
                    if (((ICollection<T>)this.InternalContainer).Count <= 0)
                    {
                        return;
                    }

                    if (!lockAcquired)
                    {
                        locker.Upgrade();
                    }

                    foreach (IUndoableItem item in this.InternalContainer.Cast<IUndoableItem>())
                    {
                        item.CaptureIntoUndoContext(this);
                    }
                }
                finally
                {
                    if (!lockAcquired)
                    {
                        locker.Dispose();
                    }
                }
            }
            else
            {
                // At this point we release the captures
                try
                {
                    if (((ICollection<T>)this.InternalContainer).Count <= 0)
                    {
                        return;
                    }

                    if (!lockAcquired)
                    {
                        locker.Upgrade();
                    }

                    foreach (IUndoableItem item in this.InternalContainer.Cast<IUndoableItem>())
                    {
                        item.ReleaseFromUndoContext();
                    }
                }
                finally
                {
                    if (!lockAcquired)
                    {
                        locker.Dispose();
                    }
                }
            }
        }
#pragma warning restore HAA0401 // Possible allocation of reference type enumerator
    }
}