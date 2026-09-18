namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`ReflTypeInfoSrc` 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `ReflTypeInfoSrc.cs`。
]
""")]
public partial class ReflTypeInfoSrc{
	[Doc("""
#Sum[取任意型別的元資料；反射來源總是「可知」。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		if(_cache.TryGetValue(Type, out Info)){
			return true;
		}
		// 直接構造而非 GetOrAdd 委託：factory 委託的參數無法帶 DAM 註解，
		// 會觸發 trimmer 警告；try-fetch 模式讓 Type 的 DAM 直接流入建構子。
		Info = new ReflTypeInfo(Type);
		// 並行下重複 TryAdd 無害（元資料只讀不可變）。
		_cache.TryAdd(Type, Info);
		return true;
	}
}