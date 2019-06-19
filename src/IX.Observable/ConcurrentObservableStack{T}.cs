// <copyright file="ConcurrentObservableStack{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using IX.Observable.DebugAide;
using IX.System.Threading;
using JetBrains.Annotations;
using GlobalThreading = System.Threading;

namespace IX.Observable
{
    /// <summary>
    ///     A thread-safe stack that broadcasts its changes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    /// <remarks>
    ///     <para>
    ///         This class is not serializable. In order to serialize / deserialize content, please use the copying methods
    ///         and serialize the result.
    ///     </para>
    /// </remarks>
    [DebuggerDisplay("ConcurrentObservableStack, Count = {Count}")]
    [DebuggerTypeProxy(typeof(StackDebugView<>))]
    [CollectionDataContract(
        Namespace = Constants.DataContractNamespace,
        Name = "Observable{0}Stack",
        ItemName = "Item")]
    [PublicAPI]
    public class ConcurrentObservableStack<T> : ObservableStack<T>
    {
        private Lazy<ReaderWriterLockSlim> locker;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        public ConcurrentObservableStack()
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(int capacity)
            : base(capacity)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(IEnumerable<T> collection)
            : base(collection)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        public ConcurrentObservableStack(GlobalThreading.SynchronizationContext context)
            : base(context)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public ConcurrentObservableStack(
            GlobalThreading.SynchronizationContext context,
            int capacity)
            : base(
                context,
                capacity)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        public ConcurrentObservableStack(
            GlobalThreading.SynchronizationContext context,
            IEnumerable<T> collection)
            : base(
                context,
                collection)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(bool suppressUndoable)
            : base(suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(
            int capacity,
            bool suppressUndoable)
            : base(
                capacity,
                suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(
            IEnumerable<T> collection,
            bool suppressUndoable)
            : base(
                collection,
                suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(
            GlobalThreading.SynchronizationContext context,
            bool suppressUndoable)
            : base(
                context,
                suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="capacity">The initial capacity of the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(
            GlobalThreading.SynchronizationContext context,
            int capacity,
            bool suppressUndoable)
            : base(
                context,
                capacity,
                suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentObservableStack{T}" /> class.
        /// </summary>
        /// <param name="context">The synchronization context top use when posting observable messages.</param>
        /// <param name="collection">A collection of items to copy into the stack.</param>
        /// <param name="suppressUndoable">If set to <see langword="true" />, suppresses undoable capabilities of this collection.</param>
        public ConcurrentObservableStack(
            GlobalThreading.SynchronizationContext context,
            IEnumerable<T> collection,
            bool suppressUndoable)
            : base(
                context,
                collection,
                suppressUndoable)
        {
            this.locker = EnvironmentSettings.GenerateDefaultLocker();
        }

        /// <summary>
        ///     Gets a synchronization lock item to be used when trying to synchronize read/write operations between threads.
        /// </summary>
        protected override IReaderWriterLock SynchronizationLock => this.locker.Value;

        /// <summary>
        ///     Called when the object is being deserialized, in order to set the locker to a new value.
        /// </summary>
        /// <param name="context">The streaming context.</param>
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context) => GlobalThreading.Interlocked.Exchange(
            ref this.locker,
            EnvironmentSettings.GenerateDefaultLocker());

        /// <summary>
        ///     Disposes the managed context.
        /// </summary>
        protected override void DisposeManagedContext()
        {
            Lazy<ReaderWriterLockSlim> l = GlobalThreading.Interlocked.Exchange(
                ref this.locker,
                null);
            if (l?.IsValueCreated ?? false)
            {
                l.Value.Dispose();
            }

            base.DisposeManagedContext();
        }

        /// <summary>
        ///     Disposes the general context.
        /// </summary>
        protected override void DisposeGeneralContext()
        {
            GlobalThreading.Interlocked.Exchange(
                ref this.locker,
                null);

            base.DisposeGeneralContext();
        }
    }
}