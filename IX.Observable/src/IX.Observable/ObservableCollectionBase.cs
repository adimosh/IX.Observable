using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace IX.Observable
{
    /// <summary>
    /// A base class for collections that are observable.
    /// </summary>
    public abstract class ObservableCollectionBase : INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// Triggers when there is a change in any of this object's properties.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// In a child class, triggers a property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            var stateTransport = new Tuple<ObservableCollectionBase, PropertyChangedEventHandler, string>(this, PropertyChanged, propertyName);
            SynchronizationContext.Current.Post((state) =>
            {
                var st = (Tuple<ObservableCollectionBase, PropertyChangedEventHandler, string>)state;
                st.Item2?.Invoke(st.Item1, new PropertyChangedEventArgs(st.Item3));
            }, stateTransport);
        }
        #endregion

        #region INotifyCollectionChanged
        /// <summary>
        /// Triggers when there is a change in the collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// In a child class, triggers a collection changed event.
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

            var stateTransport =
                new Tuple<ObservableCollectionBase, NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>
                (this, CollectionChanged, args);
            SynchronizationContext.Current.Post((state) =>
            {
                var st = (Tuple<ObservableCollectionBase, NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>)state;
                st.Item2?.Invoke(st.Item1, stateTransport.Item3);
            }, stateTransport);
        }
        #endregion
    }
}