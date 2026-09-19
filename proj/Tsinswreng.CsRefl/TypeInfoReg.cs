namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[可寫的型別元資料註冊表：手動塞 {nameof(ITypeInfo)}、支持列舉。]

#Descr[
供「反射與 JSON 都覆蓋不到」的場合
（如 CsSql 內部的 `SchemaHistory`，手寫 5 個成員的元資料即可）。

例：某型別不適合走反射、也不想掛源生成註解時，
手工造一份元資料塞進註冊表，上層代碼照常按名讀寫。
]

#Descr[
線程安全：讀寫都走 {nameof(ConcurrentDictionary<,>)}；
初始化期填完之後當只讀用也安全。

例：DI 容器裏註冊成單例，啟動時把手工元資料填進去，
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
例：{nameof(Add)} 寫這張表，{nameof(TryGetInfo)} 讀這張表，
{nameof(RegisteredTypes)} 取它的鍵快照，三者共用同一份狀態。
]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _map = new();

	[Doc($"""
#Sum[列舉已註冊型別（快照）。]

#Descr[
例：登記了 3 個型別就返回 3 個 {nameof(Type)}；
返回的是一份拷貝，之後再 {nameof(Add)} 不會改動已取出的那份清單。
]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc($"""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#Descr[
例：{nameof(Add)} 過 `typeof(User)`（還未 {nameof(Remove)}）時返回 true，
移掉之後返回 false；與 {nameof(ReflTypeInfoSrc)} 不同，本表只認登記過的型別。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
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
}