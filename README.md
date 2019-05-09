# IX.Observable

## Introduction

IX.Observable is a .NET library that seeks to implement various collection types in a manner that is observable and that can be databound to various controls or simply to provide a way to observe changes.

The motivation behind this library was a complete lack of an ObservableDictionary class that is both familiar to use and also as cross-platform as possible. Since there are various examples and code tidbits around (and even on MSDN), an available library that provides existing functionality, offers the most cross-platform options, while, at the same time, delivering performance as high as possible.

Apart from the usual collection classes (like [`List<T>`](https://msdn.microsoft.com/en-us/library/6sh2ey19.aspx), [`Dictionary<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/xfhwa508.aspx), etc.), another point was to implement a few scenarios that are used in actual software applications, but overlooked in available libraries. For instance, [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) combines multiple collections in one bindable collection, but is only available in the WPF libraries.

The project also features a very general undo/redo framework, in the form of IX.Undoable and IX.Undoable.WPF, which is implemented in IX.Observable.

## How to get

This project is primarily available through NuGet.

The current version can be accessed by using NuGet commands:

```powershell
Install-Package IX.Observable
```

Releases:
- IX.Observable: [![IX.Observable NuGet](https://img.shields.io/nuget/v/IX.Observable.svg)](https://www.nuget.org/packages/IX.Observable/)
- IX.Undoable: [![IX.Undoable NuGet](https://img.shields.io/nuget/v/IX.Undoable.svg)](https://www.nuget.org/packages/IX.Undoable/)
- IX.Undoable.WPF: [![IX.Undoable.WPF NuGet](https://img.shields.io/nuget/v/IX.Undoable.WPF.svg)](https://www.nuget.org/packages/IX.Undoable.WPF/)

## Contributing

### Guidelines

Contributing can be done by anyone, at any time and in any form, as long as the contributor
has read the [contributing guidelines](https://adimosh.github.io/contributingguidelines)
beforehand and tries their best to abide by them.

### Code health checks

| Build | Status |
|:-----:|:------:|
| Master branch | [![Build Status](https://ixiancorp.visualstudio.com/IX.Framework/_apis/build/status/IX.Observable%20master%20CI?branchName=master)](https://ixiancorp.visualstudio.com/IX.Framework/_build/latest?definitionId=8&branchName=master) |
| Continuous integration | [![Build Status](https://ixiancorp.visualstudio.com/IX.Framework/_apis/build/status/IX.Observable%20continuous%20integration?branchName=dev)](https://ixiancorp.visualstudio.com/IX.Framework/_build/latest?definitionId=7&branchName=dev) |

### Fair warning

Observable collections are not built for speed and performance.

While performance is a consideration when we build these classes, however, please be advised that they serve primarily UI-related scenarios.

If you need to use observable collections in a high-performance scenario, please either attempt re-designing your workload or use a different library instead.

## Usage

### IX.Observable

Use the available classes provided by the library as you would use the standard collection types in .NET, keeping in mind their equivalence and their special purpose.

| Class | Use as | Thread-safe | Special powers |
|:-----:|:------:|:-----------:|:--------------:|
| `ObservableDictionary<TKey, TValue>` | [`Dictionary<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2) | No | An observable dictionary that advertises both collection changes and various property changes (such as Count) |
| `ConcurrentObservableDictionary<TKey, TValue>` | [`Dictionary<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2) | Yes | Same as `ObservableDictionary<TKey, TValue>`, but also thread-safe |
| `ObservableSortedDictionary<TKey, TValue>` (not yet implemented) | [`SortedDictionary<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2) | No | An observable sorted dictionary |
| `ConcurrentSortedObservableDictionary<TKey, TValue>` (not yet implemented) | [`SortedDictionary<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2) | Yes | Same as `ObservableSortedDictionary<TKey, TValue>`, but also thread-safe |
| `ObservableStack<T>` | [`Stack<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1) | No | A stack that advertises its changes |
| `ConcurrentObservableStack<T>` | [`Stack<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1) | Yes | Same as `ObservableStack<T>`, but also thread-safe |
| `ObservableQueue<T>` | [`Queue<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1) | No | A queue that advertises its changes |
| `ConcurrentObservableQueue<T>` | [`Queue<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1) | Yes | Same as `ObservableQueue<T>`, but also thread-safe |
| `ObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.compositecollection) | No | A collection that composes multiple collections, in which one of the collections is a master and accepts updates, whereas the others are slave ones and are used for display only (note: the collections are referenced, not copied) |
| `FilterableObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.compositecollection) | No | Same as `ObservableMasterSlaveCollection<T>`, but also filterable (note: the collections are referenced, not copied) |
| `ConcurrentObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.compositecollection) | Yes | Same as `ObservableMasterSlaveCollection<T>`, but also thread-safe |
| `ConcurrentObservableCollection<T>` (not yet implemented) | [`List<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1) | Yes | A thread-safe observable list |
| `ObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedlist-2) | No | An observable sorted list |
| `ConcurrentObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedlist-2) | Yes | Same as `ObservableSortedList<T>`, but also thread-safe |
| `ObservableReadOnlyCompositeList<T>` | [`CompositeCollection`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.compositecollection) | Yes | A collection made of multiple collections that all share the same item type that advertises its changes and that does not support changes (also thread-safe by definition) |

This list will be updated as new classes are added based on ongoing work and community requests.

### IX.Undoable

The use of IX.Undoable is conditioned on using the provided base classes and interfaces.

Any object inheriting from EditableItemBase will be able to both take part in undoable operations and also be captured into an undo context.

An undo context is a context which makes both current objects and sub-objects act as a single entity. The simplest example would be a collection where undo/redo operations must take place at collection level, meaning that if a user adds, edits a field, then adds another, the undo chain would be remove, revert editing, then remove again. This, as opposed to each item being completely independent, in which case therewould be two separate chains, one with two removals and another one with a revert.

## Licenses and structure

Please be aware that this project is a sub-project of [IX.Framework](https://github.com/adimosh/IX.Framework). All credits and license information should be taken from there.