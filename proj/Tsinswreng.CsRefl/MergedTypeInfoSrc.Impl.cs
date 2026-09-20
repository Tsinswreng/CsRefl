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

#Descr[
實測：`new {nameof(MergedTypeInfoSrc)}(JsonSrc, ReflSrc)` 的優先級是 Json 源在前；
無來源（空 params）拋 {nameof(ArgumentException)}，
任一來源為 null 拋 {nameof(ArgumentNullException)}，都在構造期就暴露。

參數數組會被防禦性拷貝（實測：構造後改動調用方那份數組，合成查詢仍按原優先級命中）。
]

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

	[Doc($"""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#Descr[
實測：`typeof(PoUser)` 被第一個來源（{nameof(JsonTypeInfoSrc)}）接住，
回傳的實例與 Json 源單獨查到的 `{nameof(ReferenceEquals)}` 為 true；
未註冊的 `typeof(PoNoCtor)` 落到 {nameof(ReflTypeInfoSrc)} 兜底，回傳的是反射源那個實例；
鏈裏若沒有任何來源認識這個型別，返回 false 且 `Info` 為 null。

因為是「短路返回」，後面的來源不會被問到，
故把便宜的來源排前面能省掉不必要的解析。
]

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

	private partial IReadOnlyCollection<Type>? SnapshotTypes(){
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