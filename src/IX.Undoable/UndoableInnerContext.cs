// <copyright file="UndoableInnerContext.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using IX.StandardExtensions.ComponentModel;
using IX.System.Collections.Generic;
using JetBrains.Annotations;

namespace IX.Undoable
{
    /// <summary>
    /// The inner context of an undoable object. This class should not normally be used, instead, use <see cref="EditableItemBase"/>.
    /// </summary>
    [PublicAPI]
    public class UndoableInnerContext : NotifyPropertyChangedBase
    {
        private Lazy<PushDownStack<StateChange[]>> undoStack;
        private Lazy<PushDownStack<StateChange[]>> redoStack;

        private int historyLevels;

        /// <summary>
        /// Gets or sets the number of levels to keep undo or redo information.
        /// </summary>
        /// <value>The history levels.</value>
        /// <remarks><para>If this value is set, for example, to 7, then the implementing object should allow undo methods
        /// to be called 7 times to change the state of the object. Upon calling it an 8th time, there should be no change in the
        /// state of the object.</para>
        /// <para>Any call beyond the limit imposed here should not fail, but it should also not change the state of the object.</para></remarks>
        public int HistoryLevels
        {
            get => this.historyLevels;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                lock (this)
                {
                    // This lock is here to ensure that no multiple threads can set different history levels, no mater what.
                    // It is a write-only lock, so no need for expensive lockers
                    if (this.historyLevels == value)
                    {
                        return;
                    }

                    this.historyLevels = value;

                    this.ProcessHistoryLevelsChange();
                }

                this.RaisePropertyChanged(nameof(this.HistoryLevels));
            }
        }

        /// <summary>
        /// Gets the undo stack.
        /// </summary>
        /// <value>The undo stack.</value>
        public PushDownStack<StateChange[]> UndoStack => this.undoStack.Value;

        /// <summary>
        /// Gets the redo stack.
        /// </summary>
        /// <value>The redo stack.</value>
        public PushDownStack<StateChange[]> RedoStack => this.redoStack.Value;

        /// <summary>Disposes in the managed context.</summary>
        protected override void DisposeManagedContext()
        {
            base.DisposeManagedContext();

            // Setting history levels to 0 will automatically dispose the undo/redo stacks
            this.HistoryLevels = 0;
        }

        private void ProcessHistoryLevelsChange()
        {
            // WARNING !!! Always execute this method within a lock
            if (this.historyLevels == 0)
            {
                // Undo stack
                var stack = this.undoStack;
                this.undoStack = null;
                stack.Value.Clear();
                stack.Value.Dispose();

                // Redo stack
                stack = this.redoStack;
                this.redoStack = null;
                stack.Value.Clear();
                stack.Value.Dispose();

                // We do no null-checks as the stacks cannot be null at this point
            }
            else
            {
                // Both stacks are null at this point
                this.undoStack = new Lazy<PushDownStack<StateChange[]>>(this.GenerateStack);
                this.redoStack = new Lazy<PushDownStack<StateChange[]>>(this.GenerateStack);
            }
        }

        private PushDownStack<StateChange[]> GenerateStack()
        {
            if (this.historyLevels == 0)
            {
                throw new InvalidOperationException(Resources.NoHistoryLevelsException);
            }

            return new PushDownStack<StateChange[]>(this.historyLevels);
        }
    }
}