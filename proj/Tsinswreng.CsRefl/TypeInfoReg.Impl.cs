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
例：`Reg.{nameof(TryGetInfo)}(typeof(User), out var Info)` 在登記過時返回 true；
沒登記過返回 false 且 {nameof(Info)} 為 null（本表不兜底、不猜）。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryGetValue(Type, out Info);
	}

	[Doc($"""
#Sum[已註冊型別快照。]

#Rtn[型別列表；取一份鍵的拷貝，返回一次性列表]

#Descr[
例：登記了 3 個型別時返回 3 個 {nameof(Type)}；
因為是拷貝，之後再 {nameof(Add)} 不會影響已經取出的那份。
]
""")]
	private IReadOnlyCollection<Type>? SnapshotTypes(){
		return _map.Keys.ToList();
	}

	[Doc($"""
#Sum[登記一個型別的元資料。]

#Descr[
例：`Reg.{nameof(Add)}(typeof(Xxx), Info)` 首次成功；
再登記同一型別拋 {nameof(InvalidOperationException)}，訊息提示「請先 Remove 再 Add」。
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
例：`Reg.{nameof(Remove)}(typeof(Xxx))` 第一次返回 true，第二次返回 false。
]

#See[{nameof(ITypeInfoReg.Remove)}]
""")]
	public partial bool Remove(Type Type){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryRemove(Type, out _);
	}
}