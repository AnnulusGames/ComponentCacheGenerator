# ComponentCacheGenerator
 A source generator that automatically generates a cache of components for Unity.

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)
![unity-version](https://img.shields.io/badge/unity-2022.2+-000.svg)
[![releases](https://img.shields.io/github/release/AnnulusGames/ComponentCacheGenerator.svg)](https://github.com/AnnulusGames/ComponentCacheGenerator/releases)

[日本語版READMEはこちら](README_JA.md)

ComponentCacheGenerator provides automatic generation of component caching code for Unity by generating code to cache `GetComponent<T>()` retrievals using a source generator, reducing the amount of code you need to write.

## Setup

### Requirements

* Unity 2022.2 or later

### Installation

1. Open the Package Manager from Window > Package Manager.
2. Click the "+" button > Add package from git URL.
3. Enter the following URL:

```
https://github.com/AnnulusGames/ComponentCacheGenerator.git?path=src/ComponentCacheGenerator/Assets/ComponentCacheGenerator
```

Alternatively, open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.annulusgames.component-cache-generator": "https://github.com/AnnulusGames/ComponentCacheGenerator.git?path=src/ComponentCacheGenerator/Assets/ComponentCacheGenerator"
    }
}
```

## Basic Usage

In Unity, retrieving components with `GetComponent<T>()` is known to be costly, so it's common practice to cache references beforehand in `Awake()` or `Start()`. Additionally, `[RequireComponent(typeof(T))]` is often used to prevent forgetting to add components. While these practices are necessary for performance and maintainability, they can lead to increased code verbosity as the number of components grows.

```cs
using UnityEngine;

[RequireComponent(typeof(FooComponent))]
[RequireComponent(typeof(BarComponent))]
[RequireComponent(typeof(BazComponent))]
public class SomeBehaviour : MonoBehaviour
{
    FooComponent fooComponent;
    BarComponent barComponent;
    BazComponent bazComponent;

    void Awake()
    {
        fooComponent = GetComponent<FooComponent>();
        barComponent = GetComponent<BarComponent>();
        bazComponent = GetComponent<BazComponent>();
    }

    void Update()
    {
        fooComponent.Foo();
        barComponent.Bar();
        bazComponent.Baz();
    }
}
```

ComponentCacheGenerator automatically generates this code using the `[GenerateComponentCache]` attribute.

```cs
using UnityEngine;
using ComponentCacheGenerator;

[GenerateComponentCache(typeof(FooComponent))]
[GenerateComponentCache(typeof(BarComponent))]
[GenerateComponentCache(typeof(BazComponent))]
public partial class SomeBehaviour : MonoBehaviour
{
    void Update()
    {
        fooComponent.Foo();
        barComponent.Bar();
        bazComponent.Baz();
    }
}
```

ComponentCacheGenerator generates properties to cache each component and a `CacheComponents()` method. If the target class does not have an `Awake()` method, one will be automatically added to the class. (Note: If the target class already has an `Awake()` method, you need to manually call `CacheComponents()`.)

```cs
// Automatically added if Awake does not exist
void Awake()
{
    CacheComponents()
}
```

## Generation Options

You can specify generation code settings by specifying values for properties in the `[GenerateComponentCache]` attribute.

| Property | Description |
| - | - |
| SearchScope | Specifies the scope to search for components. If multiple are specified, the search will be performed in the order of Self > Children > Parent. (Default is Self) |
| IsRequired | If IsRequired is true, an exception will be thrown if the component is not found when `CacheComponents()` is called. If the search scope is Self, `[RequireComponent]` attribute will be automatically generated. (Default is true) |
| PropertyName | Specifies the name of the generated property. If not specified, the lower camel case version of the target component's class name will be used. |

## License

[MIT License](LICENSE)
