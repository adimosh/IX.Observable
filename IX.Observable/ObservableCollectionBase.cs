﻿// <copyright file="ObservableCollectionBase.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace IX.Observable
{
    /// <summary>
    /// A base class for collections that are observable.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    public abstract class ObservableCollectionBase : INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// The synchronization context that should be used when posting messages. This field can be null.
        /// </summary>
        private SynchronizationContext syncContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase"/> class.
        /// </summary>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableCollectionBase(SynchronizationContext context)
        {
            this.syncContext = context;
        }

        /// <summary>
        /// Triggers when there is a change in any of this object's properties.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers when there is a change in the collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Triggered when an exception has occurred during a <see cref="CollectionChanged"/> or <see cref="PropertyChanged"/> event invocation.
        /// </summary>
        public event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurredWhileNotifying;

        /// <summary>
        /// In a child class, triggers a property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (this.PropertyChanged == null)
            {
                return;
            }

            this.AsyncPost((state) => this.PropertyChanged?.Invoke(state.sender, state.args), new { sender = this, args = new PropertyChangedEventArgs(propertyName) });
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a reset change.
        /// </summary>
        protected void RaiseCollectionChanged()
        {
            if (this.CollectionChanged == null)
            {
                return;
            }

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            this.AsyncPost((state) => this.CollectionChanged?.Invoke(state.sender, state.args), new { sender = this, args });
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a list of changes.
        /// </summary>
        /// <param name="action">The change action.</param>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        /// <param name="oldIndex">The old index of the change, if any.</param>
        /// <param name="newIndex">The new index of the change, if any.</param>
        /// <remarks>
        /// <para>The developer is solely responsible for the calling of this method.</para>
        /// </remarks>
        protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList oldItems = null, IList newItems = null, int oldIndex = -1, int newIndex = -1)
        {
            if (this.CollectionChanged == null)
            {
                return;
            }

            NotifyCollectionChangedEventArgs args;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newIndex != -1)
                    {
                        args = new NotifyCollectionChangedEventArgs(action, newItems, newIndex);
                    }
                    else
                    {
                        args = new NotifyCollectionChangedEventArgs(action, newItems);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(action, oldItems, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(action, newItems, oldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    args = new NotifyCollectionChangedEventArgs(action);
                    break;
                default:
                    if (newIndex != -1 && oldIndex != -1)
                    {
                        args = new NotifyCollectionChangedEventArgs(action, oldItems, newIndex, oldIndex);
                    }
                    else
                    {
                        args = new NotifyCollectionChangedEventArgs(action);
                    }

                    break;
            }

            this.InvokeCollectionChanged(args);
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a single change.
        /// </summary>
        /// <param name="action">The change action.</param>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="oldIndex">The old index of the change, if any.</param>
        /// <param name="newIndex">The new index of the change, if any.</param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object oldItem = null, object newItem = null, int oldIndex = -1, int newIndex = -1)
        {
            if (this.CollectionChanged == null)
            {
                return;
            }

            NotifyCollectionChangedEventArgs args;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newIndex != -1)
                    {
                        args = new NotifyCollectionChangedEventArgs(action, newItem, newIndex);
                    }
                    else
                    {
                        args = new NotifyCollectionChangedEventArgs(action, newItem);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(action, oldItem, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(action, newItem, oldItem, newIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    args = new NotifyCollectionChangedEventArgs(action);
                    break;
                default:
                    if (newIndex != -1 && oldIndex != -1)
                    {
                        args = new NotifyCollectionChangedEventArgs(action, oldItem, newIndex, oldIndex);
                    }
                    else
                    {
                        args = new NotifyCollectionChangedEventArgs(action);
                    }

                    break;
            }

            this.InvokeCollectionChanged(args);
        }

        /// <summary>
        /// Asynchronously defers the execution of a method, either on the synchronization context, or on a new thread.
        /// </summary>
        /// <typeparam name="T">The type of the transport object, if any.</typeparam>
        /// <param name="postAction">The action to post.</param>
        /// <param name="stateTransport">The object used to do state transportation.</param>
        protected void AsyncPost<T>(Action<T> postAction, T stateTransport)
        {
            if (this.syncContext == null)
            {
                Task.Run(() => postAction(stateTransport)).ConfigureAwait(false);
            }
            else
            {
                this.syncContext.Post(
                    (state) =>
                {
                    var st = (T)state;
                    postAction(st);
                }, stateTransport);
            }
        }

        /// <summary>
        /// Asynchronously defers the execution of a method, either on the synchronization context, or on a new thread.
        /// </summary>
        /// <param name="postAction">The action to post.</param>
        protected void AsyncPost(Action postAction)
        {
            if (this.syncContext == null)
            {
                Task.Run(postAction);
            }
            else
            {
                this.syncContext.Post(
                    (state) =>
                {
                    postAction();
                }, null);
            }
        }

        private void InvokeCollectionChanged(NotifyCollectionChangedEventArgs args) => this.AsyncPost(
            (state) =>
            {
                try
                {
                    this.CollectionChanged?.Invoke(state.sender, state.args);
                }
                catch (Exception ex)
                {
                    this.AsyncPost(
                        (errorState) =>
                        {
                            try
                            {
                                this.ExceptionOccurredWhileNotifying?.Invoke(errorState.sender, errorState.args);
                            }
                            catch
                            {
                            }
                        }, new { sender = state.sender, args = new ExceptionOccurredEventArgs(ex) });
                }
            }, new { sender = this, args });
    }
}