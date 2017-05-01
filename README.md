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

### Fair warning

Observable collections are not built for speed and performance.

While performance is a consideration when we build these classes, however, please be advised that they serve primarily UI-related scenarios.

If you need to use observable collections in a high-performance scenario, please either attempt re-desigining your workload or use a different library instead.

## Code health
- Build status: [![Build status](https://ci.appveyor.com/api/projects/status/ir1tqpxdo9gkqj70?svg=true)](https://ci.appveyor.com/project/adimosh/ix-observable)
- Master branch status: [![Build status](https://ci.appveyor.com/api/projects/status/ir1tqpxdo9gkqj70/branch/master?svg=true)](https://ci.appveyor.com/project/adimosh/ix-observable/branch/master)

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
| `FilterableObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | Same as `ObservableMasterSlaveCollection<T>`, but also filterable (note: the collections are referenced, not copied) |
| `ConcurrentObservableMasterSlaveCollection<T>` | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | Same as `ObservableMasterSlaveCollection<T>`, but also thread-safe |
| `ConcurrentObservableCollection<T>` (not yet implemented) | [`List<T>`](https://msdn.microsoft.com/en-us/library/6sh2ey19.aspx) | A thread-safe observable list |
| `ObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132319.aspx) | An observable sorted list |
| `ConcurrentObservableSortedList<TKey, TValue>` (not yet implemented) | [`SortedList<TKey, TValue>`](https://msdn.microsoft.com/en-us/library/ms132319.aspx) | Same as `ObservableSortedList<T>`, but also thread-safe |
| `ObservableReadOnlyCompositeList<T>` (not yet implemented) | [`CompositeCollection`](https://msdn.microsoft.com/en-us/library/system.windows.data.compositecollection.aspx) | A collection made of multiple collections that all share the same item type that advertises its changes and that does not support changes (also thread-safe by definition) |

This list will be updated as new classes are added based on ongoing work and community requests.

## How to get

This project is primarily available through NuGet.

The current version can be accessed by using NuGet commands:

```powershell
Install-Package IX.Observable
```

The package is available at its [NuGet page](https://www.nuget.org/packages/IX.Observable).

## Localization

The project is too small to have an effective satellite localization, so, for now, the resources are available locally.

Currently, resources are available for:

- English (en) - also used as neutral
- Romanian (ro)
- French (fr) - auto-translated
- German (de) - auto-translated

Contributions are welcome for any other language that you would like this library to be translated into.

## Documentation

Documentation is currently in progress.

## Contributing

Contributing can be done by anyone, at any time and in any form, as long as the contributor
has read the [contributing guidelines](https://adimosh.github.io/contributingguidelines)
beforehand and tries their best to abide by them.

### Developer instructions

The project builds in Visual Studio 2017 and uses some of the language enhancements VS2017 brought. The project structure also follows the .NET Core CSPROJ standard.

Since the tooling is very difficult to work with and mostly unavailable, Visual Studio 2017 has been chosen as a suitable IDE with a good-enough project structure
for the purposes of this project. There are no plans to port this to earlier editions of Visual Studio.

Visual Studio Code should, to the extent of my knowledge, also work (at least for vanilla code changes), but I do not currently work with that IDE, instead focusing
on development with the familiar IDE that I use in commercial development at my daily job.

The project is and will be exclusive to the .NET Standard. For now, there is no point in adding further targets than the .NET Standard 1.0, which provides the
highest level of compatibility. Should any special build be required in the future, please point it out, as well as giving a reason/scenario in which things did not
work out with the current targets. Such questions and comments are always welcome, since I cannot commit to developing on all available platforms and operating systems
at the same time.

## Acknowledgements

This project uses the following libraries:

- .NET Framework Core, available from the [.NET Foundation](https://github.com/dotnet)
- StyleCop analyzer, available from [its GitHub page](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- xunit.net, available from [its GitHub page](http://xunit.github.io/)

This project uses the following tools:

- [Visual Studio](https://www.visualstudio.com/) Community Edition 2017
- GhostDoc, available at [SubMain's website](http://submain.com/products/ghostdoc.aspx)
- [Mads Kristensen](http://madskristensen.net/)'s fabulous and numerous tools and extensions, which are too many to name and are available at [his GitHub page](https://github.com/madskristensen/)

There is also [EditorConfig](http://editorconfig.org/) support and an .editorconfig file included that works with Visual Studio 2017's baked-in support.
