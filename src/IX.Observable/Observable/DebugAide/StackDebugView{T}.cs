// <copyright file="StackDebugView{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace IX.Observable.DebugAide
{
    /// <summary>
    ///     A debug view class for an observable stack.
    /// </summary>
    /// <typeparam name="T">The type of object in the stack.</typeparam>
    [UsedImplicitly]
    public sealed class StackDebugView<T>
    {
        private readonly ObservableStack<T> stack;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StackDebugView{T}" /> class.
        /// </summary>
        /// <param name="stack">The stack.</param>
        /// <exception cref="ArgumentNullException">stack is null.</exception>
        [UsedImplicitly]
        public StackDebugView(ObservableStack<T> stack)
        {
            this.stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        [UsedImplicitly]
        public T[] Items
        {
            get
            {
                var items = new T[((ICollection<T>)this.stack.InternalContainer).Count];
                this.stack.InternalContainer.CopyTo(
                    items,
                    0);
                return items;
            }
        }
    }
}