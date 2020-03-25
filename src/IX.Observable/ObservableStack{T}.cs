// <copyright file="ObservableStack{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IX.Observable.Adapters;
using IX.Observable.DebugAide;
using IX.Observable.UndoLevels;
using IX.StandardExtensions.Contracts;
using IX.System.Collections.Generic;
using IX.Undoable;
using JetBrains.Annotations;

namespace IX.Observable
{
    /// <summary>
    ///     A stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    /// <remarks>
    ///     <para>
    ///         This class is not serializable. In order to serialize / deserialize content, please use the copying methods
    ///         and serialize the result.
    ///     </para>
    /// </remarks>
    [DebuggerDisplay("ObservableStack, Count = {" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    [PublicAPI]
    public class ObservableStack<T> : ObservableCollectionBase<T>, IStack<T>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        public ObservableStack()
            : base(new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>()))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(int capacity)
            : base(new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(capacity)))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(IEnumerable<T> collection)
            : base(new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(collection)))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ObservableStack(SynchronizationContext context)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>()),
                context)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ObservableStack(
            SynchronizationContext context,
            int capacity)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(capacity)),
                context)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ObservableStack(
            SynchronizationContext context,
            IEnumerable<T> collection)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(collection)),
                context)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>()),
                suppressUndoable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(
            int capacity,
            bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(capacity)),
                suppressUndoable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(
            IEnumerable<T> collection,
            bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(collection)),
                suppressUndoable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(
            SynchronizationContext context,
            bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>()),
                context,
                suppressUndoable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(
            SynchronizationContext context,
            int capacity,
            bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(capacity)),
                context,
                suppressUndoable)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ObservableStack(
            SynchronizationContext context,
            IEnumerable<T> collection,
            bool suppressUndoable)
            : base(
                new StackCollectionAdapter<T>(new System.Collections.Generic.Stack<T>(collection)),
                context,
                suppressUndoable)
        {
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this stack is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this stack is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => this.Count == 0;

        #region Stack methods

        /// <summary>
        ///     Peeks in the stack to view the topmost item, without removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Peek()
        {
            this.RequiresNotDisposed();

            using (this.ReadLock())
            {
                return ((StackCollectionAdapter<T>)this.InternalContainer).Peek();
            }
        }

        /// <summary>
        /// Attempts to peek at the topmost item from the stack, without removing it.
        /// </summary>
        /// <param name="item">The topmost element in the stack, default if unsuccessful.</param>
        /// <returns>
        /// <see langword="true" /> if an item is peeked at successfully, <see langword="false" /> otherwise, or if the
        /// stack is empty.
        /// </returns>
        public bool TryPeek(out T item)
        {
            this.RequiresNotDisposed();

            using (this.ReadLock())
            {
                var container = (StackCollectionAdapter<T>)this.InternalContainer;

                if (container.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = container.Peek();
                return true;
            }
        }

        /// <summary>
        ///     Pops the topmost element from the stack, removing it.
        /// </summary>
        /// <returns>The topmost element in the stack, if any.</returns>
        public T Pop()
        {
            this.RequiresNotDisposed();

            T item;
            int index;

            using (this.WriteLock())
            {
                var container = (StackCollectionAdapter<T>)this.InternalContainer;
                item = container.Pop();
                index = container.Count;
            }

            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
            this.RaiseCollectionChangedRemove(
                item,
                index);

            return item;
        }

        /// <summary>
        /// Attempts to pop the topmost item from the stack, and remove it if successful.
        /// </summary>
        /// <param name="item">The topmost element in the stack, default if unsuccessful.</param>
        /// <returns>
        /// <see langword="true" /> if an item is popped successfully, <see langword="false" /> otherwise, or if the
        /// stack is empty.
        /// </returns>
        public bool TryPop(out T item)
        {
            this.RequiresNotDisposed();

            int index;

            using (var @lock = this.ReadWriteLock())
            {
                var container = (StackCollectionAdapter<T>)this.InternalContainer;

                if (container.Count == 0)
                {
                    item = default;
                    return false;
                }

                @lock.Upgrade();

                if (container.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = container.Pop();
                index = container.Count;
            }

            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
            this.RaiseCollectionChangedRemove(
                item,
                index);

            return true;
        }

        /// <summary>
        ///     Pushes an element to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            this.RequiresNotDisposed();

            int index;

            using (this.WriteLock())
            {
                var container = (StackCollectionAdapter<T>)this.InternalContainer;
                container.Push(item);
                index = container.Count - 1;
            }

            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
            this.RaiseCollectionChangedAdd(
                item,
                index);
        }

        /// <summary>
        /// Pushes a range of elements to the top of the stack.
        /// </summary>
        /// <param name="items">The item range to push.</param>
        public void PushRange(T[] items)
        {
            Contract.RequiresNotNull(in items, nameof(items));

            foreach (var item in items)
            {
                this.Push(item);
            }
        }

        /// <summary>
        /// Pushes a range of elements to the top of the stack.
        /// </summary>
        /// <param name="items">The item range to push.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to push.</param>
        public void PushRange(T[] items, int startIndex, int count)
        {
            Contract.RequiresNotNull(in items, nameof(items));
            Contract.RequiresValidArrayRange(in startIndex, in count, in items, nameof(count));

            ReadOnlySpan<T> itemsSpan = new ReadOnlySpan<T>(items, startIndex, count);
            foreach (var item in itemsSpan)
            {
                this.Push(item);
            }
        }

        /// <summary>
        ///     Copies all elements of the stack to a new array.
        /// </summary>
        /// <returns>An array containing all items in the stack.</returns>
        public T[] ToArray()
        {
            this.RequiresNotDisposed();

            using (this.ReadLock())
            {
                return ((StackCollectionAdapter<T>)this.InternalContainer).ToArray();
            }
        }

        /// <summary>
        ///     Sets the capacity to the actual number of elements in the stack if that number is less than 90 percent of current
        ///     capacity.
        /// </summary>
        public void TrimExcess()
        {
            this.RequiresNotDisposed();

            using (this.WriteLock())
            {
                ((StackCollectionAdapter<T>)this.InternalContainer).TrimExcess();
            }
        }

        #endregion

        #region Undo/Redo

        /// <summary>
        ///     Has the last undone operation redone.
        /// </summary>
        /// <param name="undoRedoLevel">A level of undo, with contents.</param>
        /// <param name="toInvokeOutsideLock">An action to invoke outside of the lock.</param>
        /// <param name="state">The state object to pass to the invocation.</param>
        /// <returns><see langword="true" /> if the redo was successful, <see langword="false" /> otherwise.</returns>
        protected override bool RedoInternally(
            StateChange undoRedoLevel,
            out Action<object> toInvokeOutsideLock,
            out object state)
        {
            if (base.UndoInternally(
                undoRedoLevel,
                out toInvokeOutsideLock,
                out state))
            {
                return true;
            }

            switch (undoRedoLevel)
            {
                case AddUndoLevel<T> aul:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    container.Push(aul.AddedItem);

                    var index = aul.Index;
                    T item = aul.AddedItem;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedAdd(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case EnqueueUndoLevel<T> eul:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    container.Push(eul.EnqueuedItem);

                    var index = container.Count - 1;
                    T item = eul.EnqueuedItem;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedAdd(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case DequeueUndoLevel<T> _:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    T item = container.Pop();
                    var index = container.Count;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedRemove(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case RemoveUndoLevel<T> _:
                {
                    toInvokeOutsideLock = null;
                    state = null;
                    break;
                }

                case ClearUndoLevel<T> _:
                {
                    this.InternalContainer.Clear();

                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (ObservableStack<T>)innerState;

                        convertedState.RaisePropertyChanged(nameof(convertedState.Count));
                        convertedState.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.RaiseCollectionReset();
                    };

                    state = this;

                    break;
                }

                default:
                {
                    toInvokeOutsideLock = null;
                    state = null;

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Has the last operation undone.
        /// </summary>
        /// <param name="undoRedoLevel">A level of undo, with contents.</param>
        /// <param name="toInvokeOutsideLock">An action to invoke outside of the lock.</param>
        /// <param name="state">The state object to pass to the invocation.</param>
        /// <returns><see langword="true" /> if the undo was successful, <see langword="false" /> otherwise.</returns>
        protected override bool UndoInternally(
            StateChange undoRedoLevel,
            out Action<object> toInvokeOutsideLock,
            out object state)
        {
            if (base.RedoInternally(
                undoRedoLevel,
                out toInvokeOutsideLock,
                out state))
            {
                return true;
            }

            switch (undoRedoLevel)
            {
                case AddUndoLevel<T> _:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    T item = container.Pop();
                    var index = container.Count;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedRemove(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case EnqueueUndoLevel<T> _:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    T item = container.Pop();
                    var index = container.Count;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedRemove(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case DequeueUndoLevel<T> dul:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;

                    container.Push(dul.DequeuedItem);

                    var index = container.Count - 1;
                    T item = dul.DequeuedItem;
                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (Tuple<ObservableStack<T>, T, int>)innerState;

                        convertedState.Item1.RaisePropertyChanged(nameof(convertedState.Item1.Count));
                        convertedState.Item1.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.Item1.RaiseCollectionChangedAdd(
                            convertedState.Item2,
                            convertedState.Item3);
                    };

                    state = new Tuple<ObservableStack<T>, T, int>(
                        this,
                        item,
                        index);

                    break;
                }

                case RemoveUndoLevel<T> _:
                {
                    toInvokeOutsideLock = null;
                    state = null;
                    break;
                }

                case ClearUndoLevel<T> cul:
                {
                    var container = (StackCollectionAdapter<T>)this.InternalContainer;
                    for (var i = 0; i < cul.OriginalItems.Length - 1; i++)
                    {
                        container.Push(cul.OriginalItems[i]);
                    }

                    toInvokeOutsideLock = innerState =>
                    {
                        var convertedState = (ObservableStack<T>)innerState;

                        convertedState.RaisePropertyChanged(nameof(convertedState.Count));
                        convertedState.RaisePropertyChanged(Constants.ItemsName);
                        convertedState.RaiseCollectionReset();
                    };

                    state = this;

                    break;
                }

                default:
                {
                    toInvokeOutsideLock = null;
                    state = null;

                    return false;
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        ///     Interprets the block state changes outside the write lock.
        /// </summary>
        /// <param name="actions">The actions to employ.</param>
        /// <param name="states">The state objects to send to the corresponding actions.</param>
        protected override void InterpretBlockStateChangesOutsideLock(
            Action<object>[] actions,
            object[] states)
        {
            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(Constants.ItemsName);
            this.RaiseCollectionReset();
        }
    }
}