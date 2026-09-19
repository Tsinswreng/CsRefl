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
例：`new {nameof(MergedTypeInfoSrc)}(JsonSrc, ReflSrc)` 的優先級是 Json 源在前；
傳空數組或其中一個為 null 都在構造期就拋，不留到查詢時才暴露。
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
例：已註冊型別被第一個來源（{nameof(JsonTypeInfoSrc)}）接住，
未註冊型別落到 {nameof(ReflTypeInfoSrc)} 兜底；
鏈裏若沒有任何來源認識這個型別，返回 false 且 {nameof(Info)} 為 null。

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

	[Doc($"""
#Sum[全部來源都支持列舉才返回並集，否則返回 null。]

#Rtn[並集快照；任一來源不支持列舉時為 null]

#Descr[
用 {nameof(HashSet<object>)} 去重（{nameof(Type)} 的相等性是引用相等，正合併集語義）；
不用 {nameof(List<object>)}.{nameof(List<object>.Contains)} 是因為那是 O(n²)，來源多的時候白燒。

例：兩個 {nameof(TypeInfoReg)} 合成的鏈，其中一個登記了 `A`、另一個登記了 `A` 與 `B`，
返回的是 `A`、`B` 兩項而不是三項。
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