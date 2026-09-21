namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源：對任意型別用反射建立 {nameof(ITypeInfo)}，並按型別緩存。]

#Descr[
AOT 下可用——前提是查詢目標的成員元數據已被保留
（見 {nameof(ReflTypeInfo.ReflDam)} 的說明）。

{nameof(RegisteredTypes)} 返回 null：
反射來源能查任意型別，無法也無需列舉。

實測：`{nameof(Inst)}.{nameof(TryGetInfo)}(typeof(PoUser), out var Info)` 與
`{nameof(Inst)}.{nameof(TryGetInfo)}(typeof(PoNoCtor), out _)` 都返回 true，
不需要事先把型別註冊到任何地方；
反過來 `{nameof(RegisteredTypes)}` 也給不出「所有能查的型別」，
因為那等於「所有型別」。

{nameof(TryGetInfo)} 實現見 `ReflTypeInfoSrc.Impl.cs`。
]
""")]
public partial class ReflTypeInfoSrc:ITypeInfoSrc{
	[Doc($"""
#Sum[型別 → 元資料緩存。]

#Descr[
同一型別只建一次（元資料構建有反射代價）。

實測：連續查 `typeof(PoUser)` 兩次拿到的是同一個 {nameof(ITypeInfo)} 實例，
故元資料與其惰性索引只建一次；
並行下重複 {nameof(ConcurrentDictionary<,>)}.{nameof(ConcurrentDictionary<,>.TryAdd)} 無害，包的是等價實例。
]
""")]
	public readonly ConcurrentDictionary<Type, ITypeInfo> _Cache = new();

	[Doc($"""
#Sum[默認單例：不經 DI 也能直接使用的反射來源。]

#Descr[
實測：`{nameof(Inst)}` 是單例（連續取兩次 `{nameof(ReferenceEquals)}` 為 true）；
不想接 DI 的場景可直接 `{nameof(ReflTypeInfoSrc)}.{nameof(Inst)}.{nameof(TryGetInfo)}(typeof(PoUser), out var Info)`；
接了 DI 的場景仍建議注入實例，方便測試時替換成別的來源。
]
""")]
	public static ReflTypeInfoSrc Inst{
		get;
	} = new();

	[Doc($"""
#Sum[不支持列舉（反射來源能查任意型別）。]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return null;
		}
	}

	[Doc($$"""
#Sum[取任意型別的元資料；反射來源總是「可知」。]

#Descr[
調用方這樣寫：

```csharp
var Refl = new ReflTypeInfoSrc();

Refl.TryGetInfo(typeof(PoUser), out var Info);   // true
Refl.TryGetInfo(typeof(PoNoCtor), out _);        // true：不必事先註冊任何型別

// 不接 DI 的場合可直接用默認單例：
ReflTypeInfoSrc.Inst.TryGetInfo(typeof(PoUser), out _);
```

除型別傳 null 之外幾乎不返回 false（故適合放在 {{nameof(MergedTypeInfoSrc)}} 的末位兜底）。
]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
}

