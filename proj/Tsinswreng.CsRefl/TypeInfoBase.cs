namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料的公共基類：把只依賴 {nameof(ITypeInfo.Members)} 的公共邏輯集中實現。]

#Descr[
集中起來的是「按名查成員」與「名清單」這些只依賴 {nameof(ITypeInfo.Members)} 的邏輯；
兩套來源只需交出 {nameof(ITypeInfo.Type)}、{nameof(ITypeInfo.Kind)}、
{nameof(ITypeInfo.Members)}、鍵值型別、實例工廠、官方 {nameof(JsonTypeInfo)}。

例：{nameof(ReflTypeInfo)} 交的是反射算出來的分類與收集到的成員，
{nameof(JsonTypeInfoInfo)} 交的是官方原樣轉發的分類與 {nameof(JsonTypeInfo.Properties)}，
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
例：{nameof(ReflTypeInfo)} 收到的是構造時傳進來的 {nameof(Type)}，
{nameof(JsonTypeInfoInfo)} 收到的是官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.Type)}，
兩者都是同一個 {nameof(Type)} 物件，可用 `==` 比較。
]
""")]
	private readonly Type _type;

	[Doc($"""
#Sum[型別分類（官方 {nameof(JsonTypeInfoKind)}）。]

#Descr[
例：Json 源直接取官方 {nameof(JsonTypeInfo.Kind)}，
反射源由分類規則算出並映射到同一個枚舉，故兩套來源的取值可比。
]
""")]
	private readonly JsonTypeInfoKind _kind;

	[Doc($"""
#Sum[成員表（契約序，已去重：遮蔽成員只留最靠近實例的那份宣告）。]

#Descr[
例：子類用 `new` 遮蔽基類成員時，這裡只有一份該名字的成員，
且它排在被遮蔽成員原先的位置；
規整由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 在建構子裏一次做完，
構造後本欄位不再變動，故按名索引可以安全緩存。
]
""")]
	private readonly IReadOnlyList<IMemberInfo> _members;

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
例：`{nameof(List<int>)}` 是 `typeof(i32)`；{nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)} 的型別為 null。
]
""")]
	private readonly Type? _elementType;

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
例：`{nameof(Dictionary<string, int>)}` 是 `typeof(str)`；
本欄位與 {nameof(ITypeInfo.ElementType)} 由同一份來源事實決定，互斥不衝突。
]
""")]
	private readonly Type? _keyType;

	[Doc($"""
#Sum[按名的成員索引緩存，首次查詢時建立。]

#Descr[
例：第一次 {nameof(TryGetMember)} 時才建這份 `Dictionary`，
之後每次按名查都是 O(1)（不是 O(n)）——O(n) 的只有枚舉 {nameof(Members)} 本身。

用 volatile 是為了多線程下雙檢：兩個線程同時建也只會多建一份等價字典，
不會看到半成品字典。
]
""")]
	private volatile Dictionary<str, IMemberInfo>? _byName;

	[Doc($"""
#Sum[可讀名清單緩存。]

#Descr[
例：第一次讀 {nameof(ReadableNames)} 時由 {nameof(Members)} 現算一次並存下來，
之後返回同一份清單實例。
]
""")]
	private volatile IReadOnlyCollection<str>? _readable;

	[Doc($"""
#Sum[可寫名清單緩存。]

#Descr[
例：成員裏有隻讀成員時，這裡的清單比 {nameof(ReadableNames)} 少那個只讀名、多隻寫名。
]
""")]
	private volatile IReadOnlyCollection<str>? _writable;

	[Doc($"""
#Sum[由派生類交出型別事實；{nameof(Members)} 會在此規整。]

#Params([
	[本元資料對應的型別],
	[型別分類（官方 {nameof(JsonTypeInfoKind)}）],
	[成員表；此處會排序去重成契約序],
	[集合的元素型別；非集合傳 null],
	[字典的鍵型別；非字典傳 null]
])

#Descr[
例：{nameof(ReflTypeInfo)} 傳的成員表是「屬性段在前、字段段在後」的收集序，
{nameof(JsonTypeInfoInfo)} 傳的是官方既有序，
兩者進來都會被規整成同一份契約序，故調用方看到的順序一致。
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

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#See[{nameof(ITypeInfo.KeyType)}]
""")]
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
例：{nameof(ReflTypeInfo)} 的判據是建構子算出的工廠是否為 null，
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
例：成員表裏 `Secret` 只讀、`Token` 只寫，
則這裡含 `Secret` 不含 `Token`；第一次讀時現算並緩存。
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
例：接上例，這裡含 `Token` 不含 `Secret`，兩份清單的交集是「可讀可寫」那批成員。
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
}