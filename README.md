# IX.Observable

## Introduction

IX.Observable is a .NET library that seeks to implement various collection types in a manner that is observable and that can be databound to various controls
or simply to provide a way to observe changes.

The motivation behind this library was a complete lack of an ObservableDictionary class that is both familiar to use and also as cross-platform as possible.
Since there are various examples and code tidbits around (and even on MSDN), an available library that provides existing functionality, offers the most
cross-platform options, while, at the same time, delivering performance as high as possible.

Apart from the usual collection classes (like [`List<T>`](https://msdn.microsoft.com/en-us/library/6sh2ey19.aspx),
[`Dictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/xfhwa508.aspx), etc.), another point was to implement a few scenarios that are used in
actual software applications, but overlooked in available libraries. For instance,
[`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) combines multiple collections in one bindable collection,
but is only available in the WPF libraries.

## Usage

Use the available classes provided by the library as you would use the standard collection types in .NET, keeping in mind their equivalence and their special
purpose.

| Class | Use as | Special powers |
|:-----:|:------:|:--------------:|
| `ObservableDictionary<TKey, TValue>` | [`Dictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/xfhwa508.aspx) | An observable dictionary that advertises both collection changes and various property changes (such as Count) |
| `ConcurrentObservableDictionary<TKey, TValue>` | [`Dictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/xfhwa508.aspx) | Same as `ObservableDictionary<TKey, TValue>`, but also thread-safe |
| `ObservableSortedDictionary<TKey, TValue>` (not yet implemented) | [`SortedDictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132259.aspx) | An observable sorted dictionary |
| `ConcurrentSortedObservableDictionary<TKey, TValue>` (not yet implemented) | [`SortedDictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132259.aspx) | Same as `ObservableSortedDictionary<TKey, TValue>`, but also thread-safe |
| `ObservableStack<T>` | [`Stack<T>`](https://msdn.microsoft.com/en-us/library/3278tedw.aspx) | A stack that advertises its changes |
| `ConcurrentObservableStack<T>` | [`Stack<T>`](https://msdn.microsoft.com/en-us/library/3278tedw.aspx) | Same as `ObservableStack<T>`, but also thread-safe |
| `ObservableQueue<T>` | [`Queue<T>`](https://msdn.microsoft.com/en-us/library/7977ey2c.aspx) | A queue that advertises its changes |
| `ConcurrentObservableQueue<T>` | [`Queue<T>`](https://msdn.microsoft.com/en-us/library/7977ey2c.aspx) | Same as `ObservableQueue<T>`, but also thread-safe |
| `ObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | A collection that composes multiple collections, in which one of the collections is a master and accepts updates, whereas the others are slave ones and are used for display only (note: the collections are referenced, not copied) |
| `ConcurrentObservableMasterSlaveCollection<T>` (not yet implemented) | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | Same as `ObservableMasterSlaveCollection<T>`, but also thread-safe |
| `ConcurrentObservableCollection<T>` (not yet implemented) | [`List<T>`](https://msdn.microsoft.com/en-us/library/6sh2ey19.aspx) | A thread-safe observable list |
| `ObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132319.aspx) | An observable sorted list |
| `ConcurrentObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132319.aspx) | Same as `ObservableSortedList<T>`, but also thread-safe |
| `ObservableReadOnlyCompositeList<T>` (not yet implemented) | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | A collection made of multiple collections that all share the same item type that advertises its changes and that does not support changes (also thread-safe by definition) |

This list is currently in progress and will be updated.

## Documentation

Documentation is currently in progress.

## Contributing

The contribution guide is currently in progress.

## Giving Thanks

This project uses the following libraries:

- .NET Framework Core, available from the [.NET Foundation](https://github.com/dotnet)
- StyleCop analyzer, available from [its GitHub page](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)

This list is currently work-in-progress and will be updated to include more.