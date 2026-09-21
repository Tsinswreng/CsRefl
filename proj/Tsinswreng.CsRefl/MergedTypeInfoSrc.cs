namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[多來源合成：按構造順序逐個查詢，第一個答「已知」的來源勝出。]

#Descr[
典型用法：
{nameof(JsonTypeInfoSrc)} 為主（AOT 主路徑、讀寫是委託），
{nameof(ReflTypeInfoSrc)} 兜底（覆蓋沒掛 `[JsonSerializable]` 的型別）。

來源各自內部緩存，本類不重建緩存。

實測：`new {nameof(MergedTypeInfoSrc)}(JsonSrc, ReflSrc)` 之後，
已註冊型別走 Json 源的源生成委託，
其餘型別落到反射源，調用方只看 {nameof(TryGetInfo)} 的結果、不必知道命中哪一個。

實測：`typeof(PoUser)` 走 Json 源，`typeof(PoNoCtor)` 落到反射源，
兩者的 `{nameof(ITypeInfo.Type)}` 都是各自查詢時傳的 `{nameof(Type)}`。
]

#Descr[
建構子、{nameof(TryGetInfo)}、列舉快照的實現見 `MergedTypeInfoSrc.Impl.cs`。

注意構造順序即優先級：{nameof(TypeInfoReg)} 必須排在 {nameof(ReflTypeInfoSrc)} 之前，
否則「全能反射」會蓋掉手工登記的元資料
（實測：`new {nameof(MergedTypeInfoSrc)}(Table, ReflSrc)` 命中的是註冊表裏那個實例）。
]
""")]
public partial class MergedTypeInfoSrc:ITypeInfoSrc{
	[Doc($"""
#Sum[來源清單，順序即優先級。]

#Descr[
建構子做防禦拷貝，構造後不受外部數組改動影響。

實測：先存下傳進去的數組，構造後把它的第 0 項換成另一個來源，
合成查詢仍按原優先級命中（因為內部存的是拷貝）。
]
""")]
	private readonly IReadOnlyList<ITypeInfoSrc> _sources;

	[Doc($"""
#Sum[全部來源都支持列舉才返回並集快照，否則返回 null。]

#Descr[
與「來源可不同構」的設計一致。

快照按訪問現算：來源本身可能在建構後繼續註冊，緩存反而會給出過期答案。

實測：鏈裏只要有一個 {nameof(JsonTypeInfoSrc)}，
整個 {nameof(RegisteredTypes)} 就是 null（它不支持列舉），
哪怕另一個 {nameof(TypeInfoReg)} 本身能列舉。

實測：兩個 {nameof(TypeInfoReg)} 合成的鏈（都支持列舉）返回並集，
同一型別登記在兩邊時只出現一次；
{nameof(JsonTypeInfoSrc)} 與 {nameof(ReflTypeInfoSrc)} 合成的鏈返回 null。
]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc($$"""
#Sum[按優先級順序給出來源。]

#Params([[Sources, 來源，順序即優先級；至少要一個]])

#Descr[
調用方這樣寫（生產就該是這一行）：

```csharp
var Src = new MergedTypeInfoSrc(
	new JsonTypeInfoSrc(TestJsonCtx.Default),   // 第一順位：掛過 [JsonSerializable] 的走源生成（零反射）
	new TypeInfoReg(),                          // 第二順位：手工登記的精確元資料（必須排在反射之前）
	new ReflTypeInfoSrc()                       // 末位：兜住其餘型別
);

Src.TryGetInfo(typeof(PoUser), out _);    // true：由 Json 源接住
Src.TryGetInfo(typeof(PoNoCtor), out _);  // true：落到反射源

new MergedTypeInfoSrc();
// 拋 ArgumentException：至少要一個來源。
```

實測：`new {{nameof(MergedTypeInfoSrc)}}(JsonSrc, Reg, ReflSrc)` 的優先級是
Json 源最高、註冊表次之、反射源兜底。
]

#See[{{nameof(MergedTypeInfoSrc)}}]
""")]
	public partial MergedTypeInfoSrc(params ITypeInfoSrc[] Sources);

	[Doc($"""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#Descr[
調用方不必知道自己命中哪一個來源：

```csharp
Src.TryGetInfo(typeof(PoUser), out var Info);
// true；Info 來自 Json 源（它排前面，故讀寫走官方委託）。

Src.TryGetInfo(typeof(SomeUnregistepedType), out _);
// false：所有來源都答「未知」時才 false。
```

實測：把優先級顛倒過來（反射源排前面），同一型別取到的就變成反射源的元資料。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ITypeInfo? Info
	);

	// ---- 私有輔助（實現見 MergedTypeInfoSrc.Impl.cs）----

	[Doc($"""
#Sum[全部來源都支持列舉才返回並集，否則返回 null。]

#Rtn[並集快照；任一來源不支持列舉時為 null]

#Descr[
用 {nameof(HashSet<object>)} 去重（{nameof(Type)} 的相等性是引用相等，正合併集語義）；
不用 {nameof(List<object>)}.{nameof(List<object>.Contains)} 是因為那是 O(n²)，來源多的時候白燒。

實測：兩個 {nameof(TypeInfoReg)} 合成的鏈，其中一個登記了 `PoColor`、
另一個登記了 `PoColor` 與 `PoUser`，
返回的是 `PoColor` 與 `PoUser` 兩項而不是三項。
]
""")]
	private partial IReadOnlyCollection<Type>? SnapshotTypes();
}









