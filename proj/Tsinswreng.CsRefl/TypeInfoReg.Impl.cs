namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(TypeInfoReg)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `TypeInfoReg.cs`。
參數特性（DAM、{nameof(NotNullWhenAttribute)}）只寫在聲明側，
`partial` 合併時兩邊都標會報 CS0579。
]
""")]
public partial class TypeInfoReg{
	[Doc($"""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#Descr[
實測：`Reg.{nameof(TryGetInfo)}(typeof(PoUser), out var Info)` 在登記過時返回 true
且 `Info` 就是塞進去的那個實例（{nameof(ReferenceEquals)} 為 true）；
未登記的型別（或移掉之後）返回 false 且 `Info` 為 null（本表不兜底、不猜）。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryGetValue(Type, out Info);
	}

	private partial IReadOnlyCollection<Type>? SnapshotTypes(){
		return _map.Keys.ToList();
	}

	[Doc($"""
#Sum[登記一個型別的元資料。]

#Descr[
實測：`Reg.{nameof(Add)}(typeof(PoUser), Info)` 首次成功；
再對同一型別 {nameof(Add)} 拋 {nameof(InvalidOperationException)}，
訊息含「已註冊」字樣（實測可用 `Contains("已註冊")` 斷言）。
]

#See[{nameof(ITypeInfoReg.Add)}]
""")]
	public partial void Add(Type Type, ITypeInfo Info){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Info);
		// 重複登記拋異常（防止無意覆蓋；確要替換先 Remove 再 Add）。
		if(!_map.TryAdd(Type, Info)){
			throw new InvalidOperationException($"型別 {Type.FullName} 已註冊，不能用 Add 覆蓋；請先 Remove 再 Add。");
		}
	}

	[Doc($"""
#Sum[移除一個型別；原本不存在返回 false。]

#Descr[
實測：登記過 `typeof(PoUser)` 後 `Reg.{nameof(Remove)}(typeof(PoUser))` 返回 true，
第二次對同一型別返回 false（本來就不在）；
移除後 `{nameof(TryGetInfo)}(typeof(PoUser), out var Info)` 返回 false 且 `Info` 為 null。
]

#See[{nameof(ITypeInfoReg.Remove)}]
""")]
	public partial bool Remove(Type Type){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryRemove(Type, out _);
	}
}