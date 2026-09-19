namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[多來源合成：按構造順序逐個查詢，第一個答「已知」的來源勝出。]

#Descr[
典型用法：
{nameof(JsonTypeInfoSrc)} 為主（AOT 主路徑、讀寫是委託），
{nameof(ReflTypeInfoSrc)} 兜底（覆蓋沒掛 `[JsonSerializable]` 的型別）。

來源各自內部緩存，本類不重建緩存。

例：`new {nameof(MergedTypeInfoSrc)}(JsonSrc, ReflSrc)` 之後，
已註冊型別走 Json 源的源生成委託，
其餘型別落到反射源，調用方只看 {nameof(TryGetInfo)} 的結果、不必知道命中哪一個。
]

#Descr[
建構子、{nameof(TryGetInfo)}、列舉快照的實現見 `MergedTypeInfoSrc.Impl.cs`。
]
""")]
public partial class MergedTypeInfoSrc:ITypeInfoSrc{
	[Doc($"""
#Sum[來源清單，順序即優先級。]

#Descr[
建構子做防禦拷貝，構造後不受外部數組改動影響。

例：`params` 數組是調用方傳進來的，若不拷貝，
調用方事後改數組元素就等於偷偷改了來源優先級。
]
""")]
	private readonly IReadOnlyList<ITypeInfoSrc> _sources;

	[Doc($"""
#Sum[全部來源都支持列舉才返回並集快照，否則返回 null。]

#Descr[
與「來源可不同構」的設計一致。

快照按訪問現算：來源本身可能在建構後繼續註冊，緩存反而會給出過期答案。

例：鏈裏只要有一個 {nameof(JsonTypeInfoSrc)}，
整個 {nameof(RegisteredTypes)} 就是 null（它不支持列舉），
哪怕另一個 {nameof(TypeInfoReg)} 本身能列舉。
]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc($"""
#Sum[按優先級順序給出來源。]

#Params([[來源，順序即優先級；至少要一個]])

#Descr[
例：`new {nameof(MergedTypeInfoSrc)}(JsonSrc, Reg, ReflSrc)` 的優先級是
Json 源最高、註冊表次之、反射源兜底；
傳空數組或傳 null 進來會在構造期就拋，不留到查詢時才暴露。
]
""")]
	public partial MergedTypeInfoSrc(params ITypeInfoSrc[] Sources);

	[Doc($"""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#Descr[
例：同一型別在 Json 源與反射源都能查，
但因 Json 源排在前面，取到的是 Json 源的元資料（讀寫走源生成委託）；
把優先級顛倒過來，同一型別取到的就變成反射源的元資料。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ITypeInfo? Info
	);
}