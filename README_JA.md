# ComponentCacheGenerator
 A source generator that automatically generates a cache of components for Unity.

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)
![unity-version](https://img.shields.io/badge/unity-2022.2+-000.svg)
[![releases](https://img.shields.io/github/release/AnnulusGames/ComponentCacheGenerator.svg)](https://github.com/AnnulusGames/ComponentCacheGenerator/releases)

[English README is here.](README.md)

ComponentCacheGeneratorはUnity用に作成されたComponentのキャッシュ処理を自動生成を提供するSource Generatorです。`GetComponent<T>()`の取得をキャッシュするコードをSource Generatorで生成することでコードの記述量を削減します。

## セットアップ

### 要件

* Unity 2022.2 以上

### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する

```
https://github.com/AnnulusGames/ComponentCacheGenerator.git?path=src/ComponentCacheGenerator/Assets/ComponentCacheGenerator
```

またはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "com.annulusgames.component-cache-generator": "https://github.com/AnnulusGames/ComponentCacheGenerator.git?path=src/ComponentCacheGenerator/Assets/ComponentCacheGenerator"
    }
}
```

## 基本的な使い方

Unityにおいて`GetComponent<T>()`でコンポーネントを取得する処理は高コストであることがよく知られており、使用する際は`Awake()`や`Start()`で参照をあらかじめキャッシュしておくことが一般的です。また、各コンポーネントの追加忘れを防止するために`[RequireComponent(typeof(T))]`を併用することも多くなります。これらはパフォーマンスや保守性の向上のために必要ですが、コンポーネントが増えていくとコードの記述量が増加していきます。

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

ComponentCacheGeneratorは`[GenerateComponentCache]`属性を使用してこれらのコードを自動生成します。

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

ComponentCacheGeneratorは各コンポーネントをキャッシュするプロパティと`CacheComponents()`メソッドを生成します。また、対象が`Awake()`メソッドを持たない場合はクラス内に自動で追加されます。(対象のクラスが既に`Awake()`を持っている場合は手動で`CacheComponents()`を呼ぶ必要があることに注意してください。)

```cs
// Awakeが存在しない場合は自動で追加される
void Awake()
{
    CacheComponents()
}
```

また、生成されるキャッシュのプロパティ名を指定することも可能です。指定がない場合には対象のコンポーネントのクラス名をlower camel caseに変換した名前が使用されます。

```cs
using UnityEngine;
using ComponentCacheGenerator;

[GenerateComponentCache(typeof(FooComponent), "foo")]
[GenerateComponentCache(typeof(BarComponent), "bar")]
[GenerateComponentCache(typeof(BazComponent), "baz")]
public partial class SomeBehaviour : MonoBehaviour
{
    void Update()
    {
        foo.Foo();
        bar.Bar();
        bazCBaz();
    }
}
```

## 生成オプション

`[GenerateComponentCache]`属性のプロパティの値を指定することで、生成コードの設定を行うことが可能です。

| プロパティ | 説明 |
| - | - |
| SearchScope | コンポーネントを探索する範囲を指定します。複数指定された場合はSelf > Children > Parentの順番で探索を行います。(デフォルトはSelf) |
| IsRequired | IsRequiredがtrueの場合、`CacheComponents()`でコンポーネントが見つからなかったときに例外をスローします。また、探索範囲がSelfの場合は`[RequireComponent]`属性を自動で生成します。(デフォルトはtrue) |

## ライセンス

[MIT License](LICENSE)