namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// ReflTypeInfoSrc 的函數實現。
public partial class ReflTypeInfoSrc{
	/// 取任意型別的元資料；反射來源總是「可知」（元數據丟失時是否拋錯由運行期決定）。
	public bool TryGetInfo(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	){
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