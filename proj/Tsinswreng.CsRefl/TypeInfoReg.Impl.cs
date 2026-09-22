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

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		return _Map.TryGetValue(Type, out Info);
	}

	[Doc($"""
#Sum[登記一個型別的元資料。]

#See[{nameof(ITypeInfoReg.Add)}]
""")]
	public partial void Add(Type Type, ITypeInfo Info){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Info);
		// 重複登記拋異常（防止無意覆蓋；確要替換先 Remove 再 Add）。
		if(!_Map.TryAdd(Type, Info)){
			throw new InvalidOperationException($"型別 {Type.FullName} 已註冊，不能用 Add 覆蓋；請先 Remove 再 Add。");
		}
	}

	[Doc($"""
#Sum[移除一個型別；原本不存在返回 false。]

#See[{nameof(ITypeInfoReg.Remove)}]
""")]
	public partial bool Remove(Type Type){
		ArgumentNullException.ThrowIfNull(Type);
		return _Map.TryRemove(Type, out _);
	}
}

