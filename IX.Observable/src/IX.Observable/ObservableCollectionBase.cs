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
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList oldItems = null, IList newItems = null)
        {
            var stateTransport =
                new Tuple<ObservableCollectionBase, NotifyCollectionChangedEventHandler, NotifyCollectionChangedAction, IList, IList>
                (this, CollectionChanged, action, oldItems, newItems);
            SynchronizationContext.Current.Post((state) =>
            {
                var st = (Tuple<ObservableCollectionBase, NotifyCollectionChangedEventHandler, NotifyCollectionChangedAction, IList, IList>)state;
                st.Item2?.Invoke(st.Item1, new NotifyCollectionChangedEventArgs(st.Item3, st.Item5, st.Item4));
            }, stateTransport);
        }
        #endregion
    }
}