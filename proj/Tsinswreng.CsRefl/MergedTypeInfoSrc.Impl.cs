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

	private partial IDictionary<Type, ITypeInfo>? SnapshotTypes(){
		// 佔位：本輪只改形狀，實現待寫（逐來源取表、按優先級合成並集）。
		throw new NotImplementedException();
	}
}

