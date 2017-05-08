// <copyright file="ObservableReadOnlyCollectionBase{T}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using IX.Observable.Adapters;

namespace IX.Observable
{
    /// <summary>
    /// A base class for read-only collections that are observable.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="global::System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="global::System.Collections.Generic.IEnumerable{T}" />
    public abstract class ObservableReadOnlyCollectionBase<T> : ObservableBase, IReadOnlyCollection<T>, ICollection
    {
        private CollectionAdapter<T> internalContainer;
        private object resetCountLocker;
        private object syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableReadOnlyCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="internalContainer">The internal container of items.</param>
        /// <param name="context">The synchronization context to use, if any.</param>
        protected ObservableReadOnlyCollectionBase(CollectionAdapter<T> internalContainer, SynchronizationContext context)
            : base(context)
        {
            this.InternalContainer = internalContainer;
            this.resetCountLocker = new object();

            this.syncRoot = new object();
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <remarks>
        /// <para>On concurrent collections, this property is read-synchronized.</para>
        /// </remarks>
        public virtual int Count => this.CheckDisposed(() => this.ReadLock(() => ((IReadOnlyCollection<T>)this.InternalContainer).Count));

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => this.CheckDisposed(() => this.ReadLock(() => this.InternalContainer.IsReadOnly));

        /// <summary>
        /// Gets a value indicating whether this instance is synchronized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets the synchronization root.
        /// </summary>
        /// <value>
        /// The synchronization root.
        /// </value>
        /// <remarks>
        /// <para>This property is inexplicably still used to &quot;synchronize&quot; access to the collections, even though MSDN members themselves
        /// admit that it was a mistake that would not be repeated with generic collections.</para>
        /// <para>It is ill-advised to use it yourself, as it does not synchronize anything.</para>
        /// <para>This property will always return an object, since UI frameworks (such as the XCeed Avalon) depend on it.</para>
        /// </remarks>
        [Obsolete]
        public object SyncRoot => this.syncRoot;

        /// <summary>
        /// Gets or sets the internal object container.
        /// </summary>
        /// <value>
        /// The internal container.
        /// </value>
        protected internal CollectionAdapter<T> InternalContainer
        {
            get => this.internalContainer;
            set
            {
                if (this.internalContainer != null)
                {
                    this.internalContainer.MustReset -= this.InternalContainer_MustReset;
                }

                this.internalContainer = value;

                if (this.internalContainer != null)
                {
                    this.internalContainer.MustReset += this.InternalContainer_MustReset;
                }
            }
        }

        /// <summary>
        /// Gets the ignore reset count.
        /// </summary>
        /// <value>
        /// The ignore reset count.
        /// </value>
        /// <remarks>
        /// <para>If this count is any number greater than zero, the <see cref="CollectionAdapter{T}.MustReset"/> event will be ignored.</para>
        /// <para>Each invocation of the collection adapter's <see cref="CollectionAdapter{T}.MustReset"/> event will decrease this counter by one until zero.</para>
        /// </remarks>
        protected int IgnoreResetCount { get; private set; }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollectionBase{T}" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ObservableCollectionBase{T}" />.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> is found in the <see cref="ObservableCollectionBase{T}" />; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>On concurrent collections, this method is read-synchronized.</para>
        /// </remarks>
        public bool Contains(T item) => this.CheckDisposed(() => this.ReadLock(() => this.InternalContainer.Contains(item)));

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollectionBase{T}" /> to an <see cref="Array" />, starting at a particular <see cref="Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}" />. The <see cref="Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <remarks>
        /// <para>On concurrent collections, this method is read-synchronized.</para>
        /// </remarks>
        public void CopyTo(T[] array, int arrayIndex) => this.CheckDisposed(() => this.ReadLock(() => this.InternalContainer.CopyTo(array, arrayIndex)));

        /// <summary>
        /// Returns a locking enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <remarks>
        /// <para>In concurrent collections, the enumerator, by default, locks the collection in place and ensures that any attempts to modify from the same thread will
        /// result in an exception, whereas from a different thread will result in the other thread patiently waiting for its turn to write.</para>
        /// <para>This implementation focuses on the normal use of enumerators, which is to dispose of their IEnumerator at the end of their enumeration cycle.</para>
        /// <para>If the enumerator is never disposed of, it will never release the read lock, thus making the other threads time out.</para>
        /// <para>Please make sure that you dispose the enumerator object at all times in order to avoid deadlocking and timeouts.</para>
        /// </remarks>
        /// <exception cref="TimeoutException">There was a timeout acquiring the necessary lock.</exception>
        public virtual IEnumerator<T> GetEnumerator()
        {
            this.CheckDisposed();

            using (this.ReadLock())
            {
                using (IEnumerator<T> enumerator = this.InternalContainer.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Copies the contents of the container to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">Index of the array.</param>
        /// <remarks>
        /// <para>On concurrent collections, this property is read-synchronized.</para>
        /// </remarks>
        void ICollection.CopyTo(Array array, int index)
        {
            this.CheckDisposed();

            T[] tempArray;

            using (this.ReadLock())
            {
                tempArray = new T[this.InternalContainer.Count - index];
                this.InternalContainer.CopyTo(tempArray, index);
            }

            tempArray.CopyTo(array, index);
        }

        /// <summary>
        /// Increases the <see cref="IgnoreResetCount"/> by one.
        /// </summary>
        protected void IncreaseIgnoreMustResetCounter()
        {
            lock (this.resetCountLocker)
            {
                this.IgnoreResetCount++;
            }
        }

        /// <summary>
        /// Increases the <see cref="IgnoreResetCount"/> by one.
        /// </summary>
        /// <param name="increaseBy">The amount to increase by.</param>
        protected void IncreaseIgnoreMustResetCounter(int increaseBy)
        {
            lock (this.resetCountLocker)
            {
                this.IgnoreResetCount += increaseBy;
            }
        }

        /// <summary>
        /// Called when the contents may have changed so that proper notifications can happen.
        /// </summary>
        protected virtual void ContentsMayHaveChanged()
        {
        }

        /// <summary>
        /// Disposes of this instance and performs necessary cleanup.
        /// </summary>
        /// <param name="managedDispose">Indicates whether or not the call came from <see cref="IDisposable"/> or from the destructor.</param>
        protected override void Dispose(bool managedDispose)
        {
            if (managedDispose)
            {
                try
                {
                    this.internalContainer.Clear();
                }
                catch
                {
                }
            }

            this.internalContainer = null;
        }

        private void InternalContainer_MustReset(object sender, EventArgs e) => this.AsyncPost(() =>
        {
            bool shouldReset;
            lock (this.resetCountLocker)
            {
                if (this.IgnoreResetCount > 0)
                {
                    this.IgnoreResetCount--;
                    shouldReset = false;
                }
                else
                {
                    shouldReset = true;
                }
            }

            if (shouldReset)
            {
                this.RaiseCollectionChanged();
                this.RaisePropertyChanged(nameof(this.Count));
                this.ContentsMayHaveChanged();
            }
        });
    }
}