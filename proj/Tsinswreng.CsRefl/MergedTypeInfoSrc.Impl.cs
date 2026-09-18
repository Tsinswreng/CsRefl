namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`MergedTypeInfoSrc` 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `MergedTypeInfoSrc.cs`。
]
""")]
public partial class MergedTypeInfoSrc{
	[Doc("""
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
		_sources = [.. Sources];
	}

	[Doc("""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		foreach(var S in _sources){
			if(S.TryGetInfo(Type, out Info)){
				return true;
			}
		}
		Info = null;
		return false;
	}

	[Doc("""
#Sum[全部來源都支持列舉才返回並集，否則返回 null。]

#Rtn[並集快照；任一來源不支持列舉時為 null]

#Descr[
用 `HashSet` 去重（`Type` 的相等性是引用相等，正合併集語義）；
不用 `List.Contains` 是因為那是 O(n²)，來源多的時候白燒。
]
""")]
	private IReadOnlyCollection<Type>? SnapshotTypes(){
		var R = new HashSet<Type>();
		foreach(var S in _sources){
			var T = S.RegisteredTypes;
			if(T is null){
				return null;
			}
			foreach(var Item in T){
				R.Add(Item);
			}
		}
		return R;
	}
}