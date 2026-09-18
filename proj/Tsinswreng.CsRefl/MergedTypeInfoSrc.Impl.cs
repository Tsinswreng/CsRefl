namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// MergedTypeInfoSrc 的函數實現。
/// 只放函數實現：字段與訪問器在 MergedTypeInfoSrc.cs。
/// 參數特性（DAM/NotNullWhen）只寫在聲明側，partial 合併時兩邊都標會報 CS0579。
public partial class MergedTypeInfoSrc{
	/// 按優先級順序給出來源。
	/// params 數組是調用方傳進來的，此處拷貝一份：
	/// 否則調用方事後改數組元素就等於偷偷改了來源優先級。
	public partial MergedTypeInfoSrc(params ITypeInfoSrc[] Sources){
		ArgumentNullException.ThrowIfNull(Sources);
		if(Sources.Length < 1){
			throw new ArgumentException("至少要一個來源。", nameof(Sources));
		}
		foreach(var S in Sources){
			ArgumentNullException.ThrowIfNull(S);
		}
		_sources = [.. Sources];
	}

	/// 第一個答「已知」的來源勝出；全部答「未知」返回 false。
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

	/// 全部來源都支持列舉才返回並集，否則返回 null。
	/// 用 HashSet 去重（Type 的相等性是引用相等，正合併集語義）；不用 List.Contains
	/// 是因為那是 O(n²)，來源多的時候白燒。
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