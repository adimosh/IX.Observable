// <copyright file="ObservableCollectionBase.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using IX.Observable.Adapters;

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
        /// Determines whether or not there are listeners to the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <returns><c>true</c> if there are no listeners, <c>false</c> otherwise.</returns>
        protected bool PropertyChangedEmpty()
        {
            return this.PropertyChanged == null;
        }

        /// <summary>
        /// In a child class, triggers a property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (this.PropertyChanged == null)
            {
                return;
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Determines whether or not there are listeners to the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <returns><c>true</c> if there are no listeners, <c>false</c> otherwise.</returns>
        protected bool CollectionChangedEmpty()
        {
            return this.CollectionChanged == null;
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a reset change.
        /// </summary>
        protected void OnCollectionChanged()
        {
            if (this.CollectionChanged == null)
            {
                return;
            }

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            this.CollectionChanged?.Invoke(this, args);
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a list of changes.
        /// </summary>
        /// <param name="action">The change action.</param>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        /// <param name="oldIndex">The old index of the change, if any.</param>
        /// <param name="newIndex">The new index of the change, if any.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList oldItems = null, IList newItems = null, int oldIndex = -1, int newIndex = -1)
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

            try
            {
                this.CollectionChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                try
                {
                    this.ExceptionOccurredWhileNotifying?.Invoke(this, new ExceptionOccurredEventArgs(ex));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a single change.
        /// </summary>
        /// <param name="action">The change action.</param>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="oldIndex">The old index of the change, if any.</param>
        /// <param name="newIndex">The new index of the change, if any.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem = null, object newItem = null, int oldIndex = -1, int newIndex = -1)
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

            try
            {
                this.CollectionChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                try
                {
                    this.ExceptionOccurredWhileNotifying?.Invoke(this, new ExceptionOccurredEventArgs(ex));
                }
                catch
                {
                }
            }
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
                Task.Run(() => postAction(stateTransport));
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
    }

    /// <summary>
    /// A base class for collections that are observable.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
#pragma warning disable SA1402 // File may only contain a single type
    public abstract class ObservableCollectionBase<T> : ObservableCollectionBase, ICollection<T>, IReadOnlyCollection<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableCollectionBase(ListAdapter<T> internalContainer, SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = internalContainer;
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public virtual int Count => ((IReadOnlyCollection<T>)this.InternalContainer).Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => this.InternalContainer.IsReadOnly;

        /// <summary>
        /// Gets or sets the internal object container.
        /// </summary>
        /// <value>
        /// The internal container.
        /// </value>
        protected internal ListAdapter<T> InternalContainer { get; set; }

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableCollectionBase{T}" />.</param>
        public virtual void Add(T item)
        {
            int newIndex = this.InternalContainer.Add(item);

            this.AsyncPost(
                (state) =>
            {
                if (state.index == -1)
                {
                    this.OnCollectionChanged();
                }
                else
                {
                    this.OnCollectionChangedAdd(state.item, state.index);
                }

                this.OnPropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();
            }, new { index = newIndex, item });
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        public virtual void Clear()
        {
            this.InternalContainer.Clear();

            this.AsyncPost(() =>
            {
                this.OnCollectionChanged();
                this.OnPropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();
            });
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollectionBase{T}" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ObservableCollectionBase{T}" />; otherwise, false.
        /// </returns>
        public virtual bool Contains(T item) => this.InternalContainer.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollectionBase{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public virtual void CopyTo(T[] array, int arrayIndex) => this.InternalContainer.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator() => this.InternalContainer.GetEnumerator();

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="ObservableCollectionBase{T}" />; otherwise, <c>false</c>. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ObservableCollectionBase{T}" />.
        /// </returns>
        public virtual bool Remove(T item)
        {
            int oldIndex = this.InternalContainer.Remove(item);

            if (oldIndex >= 0)
            {
                this.AsyncPost(
                    (state) =>
                {
                    this.OnCollectionChangedRemove(state.item, state.index);
                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                }, new { index = oldIndex, item });
                return true;
            }
            else if (oldIndex < -1)
            {
                this.AsyncPost(() =>
                {
                    this.OnCollectionChanged();
                    this.OnPropertyChanged(nameof(this.Count));
                    this.ContentsMayHaveChanged();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Called when an item is added to a collection.
        /// </summary>
        /// <param name="addedItem">The added item.</param>
        /// <param name="index">The index.</param>
        protected void OnCollectionChangedAdd(T addedItem, int index)
            => this.OnCollectionChanged(NotifyCollectionChangedAction.Add, newItem: addedItem, newIndex: index);

        /// <summary>
        /// Called when an item is removed from a collection.
        /// </summary>
        /// <param name="removedItem">The removed item.</param>
        /// <param name="index">The index.</param>
        protected void OnCollectionChangedRemove(T removedItem, int index)
            => this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem: removedItem, oldIndex: index);

        /// <summary>
        /// Called when the contents may have changed so that proper notifications can happen.
        /// </summary>
        protected virtual void ContentsMayHaveChanged()
        {
        }
    }
}