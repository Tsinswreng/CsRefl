namespace Tsinswreng.CsRefl;

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
存在的理由（代碼復用，不是抽象）：
抽象維度由 {nameof(ITypeInfo)} 接口承擔；
本類不是它的替代品，只是把兩套來源共用的成員索引緩存寫一遍。

{nameof(ITypeInfo)} 那一組屬性與按名查詢的說明見接口；
此處只寫本類新增的聲明。

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
	private readonly Type _type;

	[Doc($"""
#Sum[型別分類（官方 {nameof(JsonTypeInfoKind)}）。]

#Descr[
實測（`PoUser`）：Json 源直接取官方 {nameof(JsonTypeInfo.Kind)}，得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
反射源由分類規則算出，也是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
`typeof(List<str>)` 兩邊都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}，故兩套來源的取值可比。
]
""")]
	private readonly JsonTypeInfoKind _kind;

	[Doc($"""
#Sum[成員表（契約序，已去重：遮蔽成員只留最靠近實例的那份宣告）。]

#Descr[
實測：`PoUser` 這條鏈的成員依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`（11 項）；
另一條鏈上子類用 `new` 遮蔽基類的 `Id`，這裡是 `Name`、`Id`、`Age` 三項，
`Id` 只有一份、仍在第 2 位、按名查到的其宣告型別是子類。
規整由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 在建構子裏一次做完，
構造後本欄位不再變動，故按名索引可以安全緩存。
]
""")]
	private readonly IReadOnlyList<IMemberInfo> _members;

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
實測：`typeof(List<str>)` 的這個字段是 `typeof(str)`、`typeof(Dictionary<str, i32>)` 的是 `typeof(i32)`；
`typeof(PoUser)` 這種物件型別為 null（不是集合）。
]
""")]
	private readonly Type? _elementType;

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
實測：`typeof(Dictionary<str, i32>)` 的這個字段是 `typeof(str)`、`typeof(List<str>)` 的是 null；
本欄位與 {nameof(ITypeInfo.ElementType)} 由同一份來源事實決定，互斥不衝突。
]
""")]
//TswgNote 爲甚麼有這麼多脫褲子放屁的東西? 給我個理由?

	private readonly Type? _keyType;

	[Doc($"""
#Sum[按名的成員索引緩存，首次查詢時建立。]

#Descr[
實測：第一次 {nameof(TryGetMember)} 時才建這份 `Dictionary`，
之後每次按名查都是 O(1)（不是 O(n)）——O(n) 的只有枚舉 {nameof(Members)} 本身；
{nameof(GetMember)}("Age") 與 {nameof(TryGetMember)}("Age", out _) 返回的實例
`{nameof(ReferenceEquals)}` 為 true，即共用這份索引。

用 volatile 是為了多線程下雙檢：兩個線程同時建也只會多建一份等價字典，
不會看到半成品字典。
]
""")]
	private volatile Dictionary<str, IMemberInfo>? _byName;

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

	[Doc($"""
#Sum[由派生類交出型別事實；{nameof(Members)} 會在此規整。]

#Params([
	[Type, 本元資料對應的型別],
	[Kind, 型別分類（官方 {nameof(JsonTypeInfoKind)}）],
	[Members, 成員表；此處會排序去重成契約序],
	[ElementType, 集合的元素型別；非集合傳 null],
	[KeyType, 字典的鍵型別；非字典傳 null]
])

#Descr[
實測：{nameof(ReflTypeInfo)} 傳的成員表是「屬性段在前、字段段在後」的收集序，
{nameof(JsonTypeInfoInfo)} 傳的是官方既有序；
兩者進來都會被規整成同一份契約序，實測 `PoUser` 兩邊都是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`，
故調用方看到的順序一致。
]

#See[{nameof(TypeInfoBase)}]
""")]
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<IMemberInfo> Members,
		Type? ElementType,
		Type? KeyType
	);

	[Doc($"""
#Sum[本元資料對應的型別。]

#See[{nameof(ITypeInfo.Type)}]
""")]
	public Type Type{
		get{
			return _type;
		}
	}

	[Doc($"""
#Sum[型別分類，直接用官方 {nameof(JsonTypeInfoKind)}。]

#See[{nameof(ITypeInfo.Kind)}]
""")]
	public JsonTypeInfoKind Kind{
		get{
			return _kind;
		}
	}

	[Doc($"""
#Sum[全部成員，順序 = 契約序。]

#See[{nameof(ITypeInfo.Members)}]
""")]
	public IReadOnlyList<IMemberInfo> Members{
		get{
			return _members;
		}
	}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#See[{nameof(ITypeInfo.ElementType)}]
""")]
	public Type? ElementType{
		get{
			return _elementType;
		}
	}

	[Impl]
	public Type? KeyType{
		get{
			return _keyType;
		}
	}

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
第一次讀時現算並緩存，第二次讀返回同一份清單實例。
]

#See[{nameof(ITypeInfo.ReadableNames)}]
""")]
	public IReadOnlyCollection<str> ReadableNames{
		get{
			return _readable ??= Members.Where(M => M.CanRead).Select(M => M.Name).ToList();
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
			return _writable ??= Members.Where(M => M.CanWrite).Select(M => M.Name).ToList();
		}
	}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IMemberInfo? M);

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}，訊息含可用名清單。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name);

	// ---- 私有輔助（實現見 TypeInfoBase.Impl.cs）----

	[Doc($"""
#Sum[惰性建按名索引；已建過則直接返回。]

#Descr[
O(n) 只發生在第一次（建一次 {nameof(Dictionary<string, IMemberInfo>)}），
之後 {nameof(TryGetMember)} 與 {nameof(GetMember)} 都是 O(1)。

實測：`{nameof(GetMember)}("Age")` 與 `{nameof(TryGetMember)}("Age", out _)` 返回的
是同一實例（{nameof(ReferenceEquals)} 為 true），即共用這份索引；
名字比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)}，不受當前區域設定影響。
]
""")]
	private partial void EnsureByName();
}
