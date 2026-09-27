namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(MergedTypeInfoSrc)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `MergedTypeInfoSrc.cs`。
]
""")]
public partial class MergedTypeInfoSrc{
	[Doc($"""
#Sum[按優先級順序給出來源。]

#See[{nameof(MergedTypeInfoSrc)}]
""")]
	public partial MergedTypeInfoSrc(params ITypeInfoSrc[] Sources){
		ArgumentNullException.ThrowIfNull(Sources);
		if(Sources.Length < 1){
			throw new ArgumentException("至少要一個來源。", nameof(Sources));
		}
		foreach(var S in Sources){
			ArgumentNullException.ThrowIfNull(S);
		}
		// params 數組是調用方傳進來的，此處拷貝一份：
		// 否則調用方事後改數組元素就等於偷偷改了來源優先級。
		_Sources = [.. Sources];
	}

	[Doc($"""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		foreach(var S in _Sources){
			if(S.TryGetInfo(Type, out Info)){
				return true;
			}
		}
		Info = null;
		return false;
	}

	[Doc($"""
#Sum[逐來源取表再合成一份並集。]

#See[{nameof(RegisteredTypes)}]
""")]
	private partial IDictionary<Type, ITypeInfo>? SnapshotTypes(){
		// step 1: 逐條來源問「你的表是甚麼」；任何一條拿不出表，整條鏈就交不出並集。
		var R = new OrderedDictionary<Type, ITypeInfo>();
		foreach(var S in _Sources){
			if(S is not ITypeInfoEnumSrc E){
				return null;
			}
			var Map = E.RegisteredTypes;
			if(Map is null){
				return null;
			}
			// step 2: 併進同一份新表：排在前面的來源優先，故用 TryAdd（已有的鍵不覆蓋）。
			// step 3: null 鍵／null 值在這張表裏沒有意義，跳過而不拋（它只是「有甚麼型別」的清單）。
			foreach(var (T, Info) in Map){
				if(T is null || Info is null){
					continue;
				}
				R.TryAdd(T, Info);
			}
		}
		// step 4: 這裡現算現給，故成員來源在背後繼續登記時，下一次取值就看得到。
		return R;
	}
}

