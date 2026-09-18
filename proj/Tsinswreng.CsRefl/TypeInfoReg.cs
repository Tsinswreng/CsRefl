namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[可寫的型別元資料註冊表：手動塞 `ITypeInfo`、支持列舉。]

#Descr[
供「反射/JSON 都覆蓋不到」的場合
（如 CsSql 內部的 `SchemaHistory`，手寫 5 個成員的元資料即可）。
]

#Descr[
線程安全：讀寫都走 `ConcurrentDictionary`；
初始化期填完之後當只讀用也安全。
]

#Descr[
`TryGetInfo` / `Add` / `Remove` 的實現見 `TypeInfoReg.Impl.cs`。
]
""")]
public partial class TypeInfoReg:ITypeInfoReg{
	[Doc("""
#Sum[型別 → 元資料表。]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _map = new();

	[Doc("""
#Sum[列舉已註冊型別（快照）。]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc("""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);

	[Doc("""
#Sum[登記一個型別的元資料。]

#See[{nameof(ITypeInfoReg.Add)}]
""")]
	public partial void Add(Type Type, ITypeInfo Info);

	[Doc("""
#Sum[移除一個型別；原本不存在返回 false。]

#See[{nameof(ITypeInfoReg.Remove)}]
""")]
	public partial bool Remove(Type Type);
}