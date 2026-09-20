namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[可寫的型別元資料註冊表：手動塞 {nameof(ITypeInfo)}、支持列舉。]

#Descr[
供「反射與 JSON 都覆蓋不到」的場合
（如 CsSql 內部的 `SchemaHistory`，手寫 5 個成員的元資料即可）。

實測：把 `typeof(PoColor)` 手工建的 {nameof(ReflTypeInfo)} 塞進註冊表，
再合成到 {nameof(MergedTypeInfoSrc)} 且排在 {nameof(ReflTypeInfoSrc)} 之前時，
查 `PoColor` 拿到的就是註冊表裏那個實例（{nameof(ReferenceEquals)} 為 true），
證明手工登記確實蓋過了「現場全能反射」；
上層代碼照常按名讀寫（`Dict["Level"] = 8` 之後物件上的 `Level` 就是 8）。
]

#Descr[
線程安全：讀寫都走 {nameof(ConcurrentDictionary<,>)}；
初始化期填完之後當只讀用也安全。

實測：登記 `typeof(PoUser)` 之後再登記 `typeof(PoColor)`，兩者互不影響、都可查到；
讀寫都走 {nameof(ConcurrentDictionary<,>)}，故 DI 容器裏註冊成單例、啟動時把手工元資料填進去，
之後多線程查詢不會互相干擾。
]

#Descr[
{nameof(TryGetInfo)}、{nameof(Add)}、{nameof(Remove)} 的實現見 `TypeInfoReg.Impl.cs`。
]
""")]
public partial class TypeInfoReg:ITypeInfoReg{
	[Doc($"""
#Sum[型別 → 元資料表。]

#Descr[
實測：{nameof(Add)} 寫這張表，{nameof(TryGetInfo)} 讀這張表，
{nameof(RegisteredTypes)} 取它的鍵快照，三者共用同一份狀態。

實測：`{nameof(Add)}(typeof(PoUser), Info)` 之後
`{nameof(TryGetInfo)}(typeof(PoUser), out var Got)` 返回 true 且 `Got` 就是剛才那個 `Info`（{nameof(ReferenceEquals)} 為 true）。
]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _map = new();

	[Doc($"""
#Sum[列舉已註冊型別（快照）。]

#Descr[
實測：登記 `typeof(PoUser)` 與 `typeof(PoColor)` 後返回 2 個 {nameof(Type)}；
返回的是一份拷貝，之後再 {nameof(Add)} 不會改動已取出的那份清單。

實測：登記 `typeof(PoUser)` 與 `typeof(PoColor)` 後 {nameof(RegisteredTypes)} 的
{nameof(IReadOnlyCollection<int>)}.{nameof(IReadOnlyCollection<int>.Count)} 是 2，含這兩個型別；
之後再 `{nameof(Add)}(typeof(PoNoCtor), Info)`，先前取出的那份仍是 2（快照不受影響）。
]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc($$"""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#Descr[
調用方這樣寫：

```csharp
var Reg = new TypeInfoReg();
Reg.Add(typeof(PoUser), Info);

Reg.TryGetInfo(typeof(PoUser), out var Got);
// true；Got 就是登記進去的那個 Info（ReferenceEquals 為 true），且 Got.Type 是 typeof(PoUser)。

Reg.Remove(typeof(PoUser));
Reg.TryGetInfo(typeof(PoUser), out _);
// false：移掉之後查不到。本表只認登記過的型別，不像 {{nameof(ReflTypeInfoSrc)}} 那樣現場造一份。
```
]

#See[{{nameof(ITypeInfoSrc.TryGetInfo)}}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);

	[Doc($"""
#Sum[登記一個型別的元資料。]

#See[{nameof(ITypeInfoReg.Add)}]
""")]
	public partial void Add(Type Type, ITypeInfo Info);

	[Doc($"""
#Sum[移除一個型別；原本不存在返回 false。]

#See[{nameof(ITypeInfoReg.Remove)}]
""")]
	public partial bool Remove(Type Type);

	// ---- 私有輔助（實現見 TypeInfoReg.Impl.cs）----

	[Doc($"""
#Sum[已註冊型別快照。]

#Rtn[型別列表；鍵的一份拷貝]

#Descr[
實測：登記 `typeof(PoUser)` 與 `typeof(PoColor)` 後得到 2 個 {nameof(Type)}；
因為是拷貝，之後再 {nameof(Add)} 不會影響已經取出的那份。
]
""")]
	private partial IReadOnlyCollection<Type>? SnapshotTypes();
}
