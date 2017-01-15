﻿// <copyright file="StackDebugView.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Diagnostics;

namespace IX.Observable
{
    internal sealed class StackDebugView<T>
    {
        private readonly ObservableStack<T> stack;

        public StackDebugView(ObservableStack<T> stack)
        {
            this.stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[this.stack.InternalContainer.Count];
                this.stack.InternalContainer.CopyTo(items, 0);
                return items;
            }
        }
    }
}