namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// ReflTypeInfoSrc 的函數實現。
/// 只放函數實現：字段與訪問器在 ReflTypeInfoSrc.cs。
/// 參數特性（DAM/NotNullWhen）只寫在聲明側，partial 合併時兩邊都標會報 CS0579。
public partial class ReflTypeInfoSrc{
	/// 取任意型別的元資料；反射來源總是「可知」。
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