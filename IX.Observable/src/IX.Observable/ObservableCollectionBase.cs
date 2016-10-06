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
            syncContext = context;
        }

        /// <summary>
        /// Triggers when there is a change in any of this object's properties.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Determines whether or not there are listeners to the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <returns><c>true</c> if there are no listeners, <c>false</c> otherwise.</returns>
        protected bool PropertyChangedEmpty()
        {
            return PropertyChanged == null;
        }

        /// <summary>
        /// In a child class, triggers a property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Triggers when there is a change in the collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Determines whether or not there are listeners to the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <returns><c>true</c> if there are no listeners, <c>false</c> otherwise.</returns>
        protected bool CollectionChangedEmpty()
        {
            return CollectionChanged == null;
        }

        /// <summary>
        /// In a child class, triggers a collection changed event for a reset change.
        /// </summary>
        protected void OnCollectionChanged()
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            CollectionChanged?.Invoke(this, args);
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
            NotifyCollectionChangedEventArgs args;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newIndex != -1)
                        args = new NotifyCollectionChangedEventArgs(action, newItems, newIndex);
                    else
                        args = new NotifyCollectionChangedEventArgs(action, newItems);
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
                        args = new NotifyCollectionChangedEventArgs(action, oldItems, newIndex, oldIndex);
                    else
                        args = new NotifyCollectionChangedEventArgs(action);
                    break;
            }

            try
            {
                CollectionChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                ExceptionOccurredWhileNotifying?.Invoke(this, new ExceptionOccurredEventArgs(ex));
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
            NotifyCollectionChangedEventArgs args;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newIndex != -1)
                        args = new NotifyCollectionChangedEventArgs(action, newItem, newIndex);
                    else
                        args = new NotifyCollectionChangedEventArgs(action, newItem);
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
                        args = new NotifyCollectionChangedEventArgs(action, oldItem, newIndex, oldIndex);
                    else
                        args = new NotifyCollectionChangedEventArgs(action);
                    break;
            }

            try
            {
                CollectionChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                try
                {
                    ExceptionOccurredWhileNotifying?.Invoke(this, new ExceptionOccurredEventArgs(ex));
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Triggered when an exception has occurred during a <see cref="CollectionChanged"/> event is invoked.
        /// </summary>
        public event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurredWhileNotifying;

        /// <summary>
        /// Asynchronously defers the execution of a method, either on the synchronization context, or on a new thread.
        /// </summary>
        /// <typeparam name="T">The type of the transport object, if any.</typeparam>
        /// <param name="postAction">The action to post.</param>
        /// <param name="stateTransport">The object used to do state transportation.</param>
        protected void AsyncPost<T>(Action<T> postAction, T stateTransport)
        {
            if (syncContext == null)
            {
                Task.Run(() => postAction(stateTransport));
            }
            else
            {
                syncContext.Post((state) =>
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
            if (syncContext == null)
            {
                Task.Run(postAction);
            }
            else
            {
                syncContext.Post((state) =>
                {
                    postAction();
                }, null);
            }
        }
    }
}