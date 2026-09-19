namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ReflTypeInfoSrc)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `ReflTypeInfoSrc.cs`。
]
""")]
public partial class ReflTypeInfoSrc{
	[Doc($"""
#Sum[取任意型別的元資料；反射來源總是「可知」。]

#Descr[
實測：同一型別查兩次拿到同一個 {nameof(ITypeInfo)} 實例（`{nameof(ReferenceEquals)}` 為 true），
故元資料與其惰性按名索引只建一次；第二次查直接命中緩存，不再走反射。

這裡刻意不用 {nameof(System.Collections.Concurrent.ConcurrentDictionary<,>)}
.{nameof(System.Collections.Concurrent.ConcurrentDictionary<,>.GetOrAdd)} 的委託重載：
factory 委託的參數無法帶 DAM 註解，會觸發 trimmer 警告；
先查後建的寫法讓 {nameof(Type)} 的 DAM 直接流進建構子。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		// step 1: 命中緩存直接返回，避免重複付反射建元資料的代價。
		if(_cache.TryGetValue(Type, out Info)){
			return true;
		}
		// step 2: 直接構造而非 GetOrAdd 委託：factory 委託的參數無法帶 DAM 註解，
		// 會觸發 trimmer 警告；try-fetch 模式讓 Type 的 DAM 直接流入建構子。
		Info = new ReflTypeInfo(Type);
		// step 3: 收進緩存；並行下重複 TryAdd 無害（元資料只讀不可變）。
		_cache.TryAdd(Type, Info);
		return true;
	}
}