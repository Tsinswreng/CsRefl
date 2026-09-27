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
public partial class MergedTypeInfoSrc:ITypeInfoEnumSrc{
	[Doc($"""
#Sum[來源清單，順序即優先級。]

#Descr[
建構子做防禦拷貝，構造後不受外部數組改動影響。

實測：先存下傳進去的數組，構造後把它的第 0 項換成另一個來源，
合成查詢仍按原優先級命中（因為內部存的是拷貝）。
]
""")]
	public readonly IReadOnlyList<ITypeInfoSrc> _Sources;

	[Doc($"""
#Sum[賦值進來的列舉覆蓋表；非 null 時 {nameof(RegisteredTypes)} 交出它。]

#Descr[
本類自己沒有表，故賦值只能記在這裡、由 {nameof(RegisteredTypes)} 的取值優先交出。
它不參與 {nameof(TryGetInfo)}：查詢用哪些來源仍由 {nameof(_Sources)} 決定，賦值改不了查詢結果。
]
""")]
	public IDictionary<Type, ITypeInfo>? _RegisteredTypes;

	[Doc($"""
#Sum[成員來源都拿得出表時，現算一份並集。]

#Descr[
本類自己沒有表：它按順序問每一條成員來源「你的表是甚麼」，把各張表併成一份新字典。
同一個型別被多條來源登記時只留一份，保留排在最前面那條來源的元資料。
按訪問現算，不緩存：成員來源可以在本類背後繼續登記，緩存會給出過期的答案。

成員來源之中只要有一條拿不出表（例如 {nameof(JsonTypeInfoSrc)} 與 {nameof(ReflTypeInfoSrc)}），
本屬性就交不出並集，此時值是 null；哪怕另一個 {nameof(TypeInfoReg)} 本身拿得出表。

賦值＝覆蓋列舉結果：本類沒有自己的表，故賦值只影響列舉，不改查詢的來源優先級。
]

#See[{nameof(ITypeInfoEnumSrc.RegisteredTypes)}]
""")]
	public IDictionary<Type, ITypeInfo>? RegisteredTypes{
		get{
			// 有賦值過就交出賦值的那一份；否則現算成員來源的並集（本類自己沒有表）。
			return _RegisteredTypes ?? SnapshotTypes();
		}
		set{
			// 本類沒有自己的表：記下來只影響列舉，查詢的來源優先級不受影響（見 _RegisteredTypes）。
			_RegisteredTypes = value;
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
#Sum[逐條來源取表再合成一份並集；{nameof(RegisteredTypes)} 的取值就是呼叫本方法。]

#Rtn[一個新字典，鍵是全部來源已知的型別，值是該型別的元資料；只要有一條來源不支持列舉就回 null]

#Descr[
本方法依序問每一條來源「你的表是甚麼」，再把各張表裏的鍵值對併進同一個新字典。
同一個型別被多條來源登記時只留一份，保留排在最前面那條來源給的元資料，
因為排在最前面代表優先級最高。

只要有一條來源回答「我沒有表」，本方法就回 null，
也就是這條合成鏈不能當型別全集用。
]
""")]
	private partial IDictionary<Type, ITypeInfo>? SnapshotTypes();
}









