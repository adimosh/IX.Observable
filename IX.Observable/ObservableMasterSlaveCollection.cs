using IX.Observable.Adapters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IX.Observable
{
    /// <summary>
    /// An observable collection created from a master collection (to which updates go) and many slave, read-only collections.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="IX.Observable.ObservableCollectionBase{TItem}" />
    public class ObservableMasterSlaveCollection<T> : ObservableCollectionBase<T>
    {
        public ObservableMasterSlaveCollection(SynchronizationContext context)
            : base(new MultiListListAdapter<T>(), context)
        {

        }
    }
}