namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料的公共基類：把只依賴 {nameof(ITypeInfo.Members)} 的公共邏輯集中實現。]

#Descr[
集中起來的是「按名查成員」與「名清單」這些只依賴 {nameof(ITypeInfo.Members)} 的邏輯；
兩套來源只需交出 {nameof(ITypeInfo.Type)}、{nameof(ITypeInfo.Kind)}、
{nameof(ITypeInfo.Members)}、鍵值型別、實例工廠、官方 {nameof(JsonTypeInfo)}。

實測：{nameof(ReflTypeInfo)} 交的是反射算出來的分類與收集到的成員，
{nameof(JsonTypeInfoInfo)} 交的是官方原樣轉發的分類與 {nameof(JsonTypeInfo.Properties)}，
兩者交出來的內容實測逐位相同（`PoUser` 都是 11 項、首位都是 `Id`）；
之後按名索引、名清單、去重全走同一份代碼。
]

#Descr[
存在的理由（代碼復用、以及把 O(1) 的按名索引與名清單緩存放一處，不是抽象）：
抽象維度由 {nameof(ITypeInfo)} 接口承擔；本類不是它的替代品。

構造期算出來的事實一律直接落在對應屬性上（自動屬性），不再另存一份欄位由屬性轉發；
本類只留三個真正的緩存欄位：`_byName`、`_readable`、`_writable`。

建構子與按名查詢的實現見 `TypeInfoBase.Impl.cs`。
]
""")]
public abstract partial class TypeInfoBase:ITypeInfo{
	[Doc($"""
#Sum[本元資料對應的型別。]

#Descr[
實測：查 `PoUser` 時，{nameof(ReflTypeInfo)} 收到的是構造時傳進來的 `typeof(PoUser)`，
{nameof(JsonTypeInfoInfo)} 收到的是官方 {nameof(JsonTypeInfo.Type)}（也是 `typeof(PoUser)`）；
兩者是同一個 {nameof(Type)} 物件，`{nameof(ReferenceEquals)}` 為 true，故可用 `==` 比較。
]
""")]
	public Type Type{
		get;
	}

	[Doc($"""
#Sum[型別分類（官方 {nameof(JsonTypeInfoKind)}）。]

#Descr[
實測（`PoUser`）：Json 源直接取官方 {nameof(JsonTypeInfo.Kind)}，得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
反射源由分類規則算出，也是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
`typeof(List<str>)` 兩邊都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}，故兩套來源的取值可比。
]
""")]
	public JsonTypeInfoKind Kind{
		get;
	}

	[Doc($"""
#Sum[成員表（契約序，已去重：遮蔽成員只留最靠近實例的那份宣告）。]

#Descr[
實測：`PoUser` 這條鏈的成員依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`（11 項）；
另一條鏈上子類用 `new` 遮蔽基類的 `Id`，這裡是 `Name`、`Id`、`Age` 三項，
`Id` 只有一份、仍在第 2 位、按名查到的其宣告型別是子類。
規整由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 在建構子裏一次做完，
構造後本屬性不再變動，故按名索引與名清單可以安全緩存。
]
""")]
	public IReadOnlyList<obj?> Members{
		get;
	}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
實測：`typeof(List<str>)` 的這個屬性是 `typeof(str)`、`typeof(Dictionary<str, i32>)` 的是 `typeof(i32)`；
`typeof(PoUser)` 這種物件型別為 null（不是集合）。
]
""")]
	public Type? ElementType{
		get;
	}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
實測：`typeof(Dictionary<str, i32>)` 的這個屬性是 `typeof(str)`、`typeof(List<str>)` 的是 null；
本屬性與 {nameof(ElementType)} 由同一份來源事實決定，互斥不衝突。
]
""")]
	//TswgNote 爲甚麼有這麼多脫褲子放屁的東西? 給我個理由?
	// 已按此清掉：構造期算出來的事實直接落在屬性上（自動屬性），不再另存欄位由屬性轉發。
	public Type? KeyType{
		get;
	}

	[Doc($"""
#Sum[按名的成員索引緩存，首次查詢時建立。]

#Descr[
實測：第一次 {nameof(TryGetMember)} 時才建這份 {nameof(Dictionary<,>)}，
之後每次按名查都是 O(1)（不是 O(n)）——O(n) 的只有枚舉 {nameof(Members)} 本身；
{nameof(GetMember)}("Age") 與 {nameof(TryGetMember)}("Age", out _) 返回的實例
`{nameof(ReferenceEquals)}` 為 true，即共用這份索引。

用 volatile 是為了多線程下雙檢：兩個線程同時建也只會多建一份等價字典，
不會看到半成品字典。
]
""")]
	private volatile Dictionary<str, obj?>? _byName;

	[Doc($"""
#Sum[可讀名清單緩存。]

#Descr[
實測（`PoUser`）：第一次讀 {nameof(ReadableNames)} 時由 {nameof(Members)} 現算一次並存下來，
內容是 10 個名（跳過只寫的 `Token`）；
第二次讀返回的是同一份清單實例（`{nameof(ReferenceEquals)}` 為 true）。
]
""")]
	private volatile IReadOnlyCollection<str>? _readable;

	[Doc($"""
#Sum[可寫名清單緩存。]

#Descr[
實測（`PoUser`）：這份清單是 10 個名（跳過只讀的 `Secret`、含只寫的 `Token`），
與 {nameof(ReadableNames)} 的差別只有一處：把 `Secret` 換成了 `Token`。
]
""")]
	private volatile IReadOnlyCollection<str>? _writable;

	[Doc($$"""
#Sum[由派生類交出型別事實；{{nameof(Members)}} 會在此規整。]

#Params([
	[Type, 本元資料對應的型別],
	[Kind, 型別分類（官方 {{nameof(JsonTypeInfoKind)}}）],
	[Members, 成員表；此處會排序去重成契約序],
	[ElementType, 集合的元素型別；非集合傳 null],
	[KeyType, 字典的鍵型別；非字典傳 null]
])

#Descr[
派生類是這樣交事實的（本包的 {{nameof(ReflTypeInfo)}} 與 {{nameof(JsonTypeInfoInfo)}} 就是這兩種寫法）：

```csharp
public partial class MyInfo:TypeInfoBase{
	public partial MyInfo(Type T)
		: base(
			Type: T,
			Kind: JsonTypeInfoKind.Object,
			Members: new obj?[]{ typeof(MyInfo).GetProperty(nameof(MyInfo.Tag))! },
			ElementType: null,
			KeyType: null
		){
	}

	public i32 Tag{get;set;}
}
// 進來之後 Members 已被規整成契約序：基類在前、同類內宣告序、同名只留最靠近實例的那份。
```

{{nameof(ReflTypeInfo)}} 傳的成員表是「屬性段在前、字段段在後」的收集序，
{{nameof(JsonTypeInfoInfo)}} 傳的是官方既有序；兩者進來都會被規整成同一份契約序。
]

#See[{{nameof(TypeInfoBase)}}]
""")]
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<obj?> Members,
		Type? ElementType,
		Type? KeyType
	);

	[Doc($"""
#Sum[無參實例工廠（官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 的形狀）；兩套來源各自提供。]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public abstract Func<obj>? CreateObject{get;}

	[Doc($"""
#Sum[被包裝的官方 {nameof(JsonTypeInfo)}；反射來源為 null。兩套來源各自提供。]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public abstract JsonTypeInfo? Json{get;}

	[Doc($"""
#Sum[本型別能否建立無參實例。]

#Descr[
實測：`typeof(PoUser)` 兩套來源都是 true；`typeof(PoNoCtor)` 兩套來源都是 false。
{nameof(ReflTypeInfo)} 的判據是建構子算出的工廠是否為 null，
{nameof(JsonTypeInfoInfo)} 的判據是官方 {nameof(JsonTypeInfo.CreateObject)} 是否為 null，
兩者都歸到「{nameof(CreateObject)} 是否為 null」這一條。
]

#See[{nameof(ITypeInfo.CanMkInst)}]
""")]
	public bool CanMkInst{
		get{
			return CreateObject is not null;
		}
	}

	[Doc($"""
#Sum[建立一個無參實例（轉調 {nameof(CreateObject)}）；不可建時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public abstract obj? MkInst();

	[Doc($"""
#Sum[可讀成員名清單，順序同 {nameof(Members)}（已去重，不含重複名）。]

#Descr[
實測（`PoUser`）：`Secret` 只讀、`Token` 只寫，
故這裡是 10 個名、含 `Secret` 不含 `Token`；
第一次讀時現算並緩存，第二次讀返回同一份清單實例，故在循環裏反復讀不會反復計算。
]

#See[{nameof(ITypeInfo.ReadableNames)}]
""")]
	public IReadOnlyCollection<str> ReadableNames{
		get{
			// 惰性算一次並緩存：成員表構造後不變，故緩存安全（見 _readable）。
			return _readable ??= Members
				.Where(M => Member.CanRead(M))
				.Select(M => Member.Name(M))
				.ToList();
		}
	}

	[Doc($"""
#Sum[可寫成員名清單，順序同 {nameof(Members)}（已去重，不含重複名）。]

#Descr[
實測（`PoUser`）：這裡也是 10 個名、含 `Token` 不含 `Secret`；
兩份清單的交集是 9 個「可讀可寫」成員名，正好等於 {nameof(InstDict)} 的鍵表。
]

#See[{nameof(ITypeInfo.WritableNames)}]
""")]
	public IReadOnlyCollection<str> WritableNames{
		get{
			// 同上，惰性算一次並緩存（見 _writable）。
			return _writable ??= Members
				.Where(M => Member.CanWrite(M))
				.Select(M => Member.Name(M))
				.ToList();
		}
	}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#Descr[
走 {nameof(_byName)} 那份惰性索引，是 O(1)（見 {nameof(_byName)}），不掃 {nameof(Members)}。
]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, [NotNullWhen(true)] out obj? M);

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}，訊息含可用名清單。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial obj? GetMember(str Name);

	// ---- 私有輔助（實現見 TypeInfoBase.Impl.cs）----

	[Doc($"""
#Sum[惰性建按名索引；已建過則直接返回。]

#Descr[
O(n) 只發生在第一次（建一次 {nameof(Dictionary<,>)}），
之後 {nameof(TryGetMember)} 與 {nameof(GetMember)} 都是 O(1)。

實測：`{nameof(GetMember)}("Age")` 與 `{nameof(TryGetMember)}("Age", out _)` 返回的
是同一實例（{nameof(ReferenceEquals)} 為 true），即共用這份索引；
名字比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)}，不受當前區域設定影響。
]
""")]
	private partial void EnsureByName();

	[Doc($"""
#Sum[按成員序列出全部成員名，供未命中時的錯誤訊息用。]

#Descr[
實測（`PoUser`）：11 個名，與成員表同序，首位是 `Id`、末位是 `Note`。
]
""")]
	private partial IEnumerable<str> AllNames();

	[Doc($"""
#Sum[見接口說明。]
""")]
	public partial bool TryGetMemberType(str Name, out Type? T);
	[Doc($"""
#Sum[見接口說明。]
""")]
	public partial bool CanRead(str Name);
	[Doc($"""
#Sum[見接口說明。]
""")]
	public partial bool CanWrite(str Name);
}


