namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[官方 {nameof(JsonTypeInfo)} 的薄配接器：把官方元資料接到 {nameof(ITypeInfo)} 上。]

#Descr[
薄的定義：凡是官方已有的，一律原樣轉發、一個字不算——
{nameof(ITypeInfo.Type)} 取官方 {nameof(JsonTypeInfo.Type)}、
{nameof(ITypeInfo.Kind)} 取官方 {nameof(JsonTypeInfo.Kind)}、
{nameof(ITypeInfo.Members)} 取官方 {nameof(JsonTypeInfo.Properties)}、
{nameof(ITypeInfo.ElementType)} 與 {nameof(ITypeInfo.KeyType)} 同名同義、
{nameof(ITypeInfo.CreateObject)} 直接就是官方那條委託。

為甚麼需要這一層：官方 {nameof(JsonTypeInfo)} 是官方的類，本包改不了它的宣告，
而源生成來源必須交出一份 {nameof(ITypeInfo)}。

實測：`typeof(PoUser)` 經本類包裝後，{nameof(Members)} 的每一項都是包住官方 {nameof(JsonPropertyInfo)} 的
{nameof(JsonMemberInfo)}（首項是 `Id`、末項是 `Note`），{nameof(Json)} 就是傳進來的那個官方實例
（{nameof(ReferenceEquals)} 為 true）。

建構子與 {nameof(MkInst)} 的實現見 `JsonTypeInfoInfo.Impl.cs`。
]
""")]
public partial class JsonTypeInfoInfo:ITypeInfo{
	[Doc($$"""
#Sum[用官方型別元資料建配接器。]

#Params([[Json, 官方型別元資料；不允許 null]])

#Descr[
調用方通常不直接 new，而是從 {{nameof(JsonTypeInfoSrc)}} 拿；
要自己包時這樣寫：

```csharp
var Json = TestJsonCtx.Default.GetTypeInfo(typeof(PoUser))!;
var Info = new JsonTypeInfoInfo(Json);

Info.Kind;            // JsonTypeInfoKind.Object
Info.Members.Count;   // 11，每一項都是包住官方 JsonPropertyInfo 的 JsonMemberInfo
Info.GetMember(nameof(PoUser.Age));

new JsonTypeInfoInfo(null!);
// 拋 ArgumentNullException（構造期就擋住，不留到查詢時）。
```

標量與集合沒有成員：包 `typeof(List<str>)` 時 {{nameof(ITypeInfo.Members)}} 為空、
{{nameof(ITypeInfo.ElementType)}} 是 `typeof(str)`。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json);

	// ---- 型別事實：建構子一次算好，直接落在屬性上 ----

	[Doc($"""
#Sum[本元資料對應的型別。]

#Descr[
實測：查 `PoUser` 時，{nameof(ReflTypeInfo)} 收到的是構造時傳進來的 `typeof(PoUser)`，
{nameof(JsonTypeInfoInfo)} 收到的是官方 {nameof(JsonTypeInfo.Type)}（也是 `typeof(PoUser)`）；
兩者是同一個 {nameof(Type)} 物件，`{nameof(ReferenceEquals)}` 為 true，故可用 `==` 比較。
]
""")]
	public Type Type{get;set;}

	[Doc($"""
#Sum[型別分類（官方 {nameof(JsonTypeInfoKind)}）。]

#Descr[
實測（`PoUser`）：Json 源直接取官方 {nameof(JsonTypeInfo.Kind)}，得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
反射源由分類規則算出，也是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
`typeof(List<str>)` 兩邊都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}，故兩套來源的取值可比。
]
""")]
	public JsonTypeInfoKind Kind{get;set;}

	[Doc($"""
#Sum[成員表：鍵是成員名、值是成員本體；契約序，已去重。]

#Descr[
實測：`PoUser` 這條鏈的成員依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`（11 項）；
另一條鏈上子類用 `new` 遮蔽基類的 `Id`，這裡是 `Name`、`Id`、`Age` 三項，
`Id` 只有一份、仍在第 2 位、按名查到的其宣告型別是子類。

規整由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 在建構子裏一次做完，
再依序放進 {nameof(OrderedDictionary<,>)}：插入序就是契約序，故枚舉順序即契約序，
按名查就是它的 TryGetValue。

本屬性可賦值（換整張成員表）；賦值不重建三份子集快取，故換表通常該重建一份實例。
]
""")]
	public IDictionary<str, IMemberInfo> Members{get;set;}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
實測：`typeof(List<str>)` 的這個屬性是 `typeof(str)`、`typeof(Dictionary<str, i32>)` 的是 `typeof(i32)`；
`typeof(PoUser)` 這種物件型別為 null（不是集合）。
]
""")]
	public Type? ElementType{get;set;}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
實測：`typeof(Dictionary<str, i32>)` 的這個屬性是 `typeof(str)`、`typeof(List<str>)` 的是 null；
本屬性與 {nameof(ElementType)} 由同一份來源事實決定，互斥不衝突。
]
""")]
	public Type? KeyType{get;set;}

	// ---- 惰性快取：第一次用到時才建，之後一直用 ----

	[Doc($"""
#Sum[可讀成員表緩存（本庫持有的那一份）。]

#Descr[
實測（`PoUser`）：第一次讀 {nameof(ReadableMembers)} 時由 {nameof(Members)} 現算一次並存下來，
內容是 10 項（跳過只寫的 `Token`）；
第二次讀返回的是同一份表實例（`{nameof(ReferenceEquals)}` 為 true）。
]
""")]
	public volatile IDictionary<str, IMemberInfo>? _Readable;

	[Doc($"""
#Sum[可寫成員表緩存（本庫持有的那一份）。]

#Descr[
實測（`PoUser`）：這份表是 10 項（跳過只讀的 `Secret`、含只寫的 `Token`），
與 {nameof(ReadableMembers)} 的差別只有一處：把 `Secret` 換成了 `Token`。
]
""")]
	public volatile IDictionary<str, IMemberInfo>? _Writable;

	[Doc($"""
#Sum[可讀且可寫成員表緩存（本庫持有的那一份）。]

#Descr[
實測（`PoUser`）：這份表是 9 項，正好等於 {nameof(InstDict)} 的鍵表。
]
""")]
	public volatile IDictionary<str, IMemberInfo>? _ReadWrite;

	[Doc($"""
#Sum[無參實例工廠；沒賦值過時現讀官方本體那條委託。]

#Descr[
沒賦值過時刻意是「現讀」而不是構造期快照：官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)}
是可寫的，官方本體被換掉時門面要跟著變，才有資格叫薄配接器。

賦值＝記住一條覆蓋委託，之後本屬性就交出它、不再現讀官方，也不去動官方那個共享實例。
null 本身是合法取值（表示本型別沒有無參工廠），故另用 {nameof(_HasCreateObject)} 區分「賦值過」與「沒賦值過」。
]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public Func<obj>? CreateObject{
		get{
			// 賦值過就以覆蓋值為準；沒賦值過才現讀官方本體。
			return _HasCreateObject ? _CreateObject : Json?.CreateObject;
		}
		set{
			// 賦值＝記下覆蓋值（見本屬性的說明）；不寫回官方本體，免得改到別處也在用的那個官方實例。
			_CreateObject = value;
			_HasCreateObject = true;
		}
	}

	[Doc($"""
#Sum[{nameof(CreateObject)} 的覆蓋值；沒賦值過時無意義。]

#Descr[
與 {nameof(_HasCreateObject)} 配對使用：因為 null 是合法取值，單看本欄位分不出「沒賦值」與「賦了 null」。
]
""")]
	public Func<obj>? _CreateObject;

	[Doc($"""
#Sum[{nameof(CreateObject)} 有沒有被賦值過。]

#Descr[
true 表示 {nameof(CreateObject)} 交出的是 {nameof(_CreateObject)}（哪怕是 null），false 表示現讀官方本體。
]
""")]
	public bool _HasCreateObject;

	[Doc($"""
#Sum[官方型別元資料本體；構造期賦值，可再賦值。]

#Descr[
就是構造時傳進來的那個官方實例（{nameof(ReferenceEquals)} 為 true），
{nameof(Members)} 與 {nameof(CreateObject)} 都以它為源。
]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public JsonTypeInfo? Json{get;set;}

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
#Sum[建立無參實例；官方沒有工廠時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public partial obj? MkInst();

	[Doc($"""
#Sum[可讀成員表，順序同 {nameof(Members)}（已去重，不含重複名）。]

#Descr[
實測（`PoUser`）：`Secret` 只讀、`Token` 只寫，
故這裡是 10 項、含 `Secret` 不含 `Token`；
第一次讀時現算並緩存，第二次讀返回同一份表實例，故在循環裏反復讀不會反復計算。
]

#See[{nameof(ITypeInfo.ReadableMembers)}]
""")]
	public IDictionary<str, IMemberInfo> ReadableMembers{
		get{
			// 惰性算一次並緩存：成員表構造後不變，故緩存安全（見 _Readable）。
			return _Readable ??= MkSubset(M => M.CanRead);
		}
	}

	[Doc($"""
#Sum[可寫成員表，順序同 {nameof(Members)}（已去重，不含重複名）。]

#Descr[
實測（`PoUser`）：這裡也是 10 項、含 `Token` 不含 `Secret`；
第一次讀時現算並緩存，第二次讀返回同一份表實例。
]

#See[{nameof(ITypeInfo.WritableMembers)}]
""")]
	public IDictionary<str, IMemberInfo> WritableMembers{
		get{
			// 同上，惰性算一次並緩存（見 _Writable）。
			return _Writable ??= MkSubset(M => M.CanWrite);
		}
	}

	[Doc($"""
#Sum[可讀且可寫成員表（列與表單欄位就是這一批），順序同 {nameof(Members)}。]

#Descr[
實測（`PoUser`）：這裡是 9 項，正好等於 {nameof(InstDict)} 的鍵表。
]

#See[{nameof(ITypeInfo.ReadWriteMembers)}]
""")]
	public IDictionary<str, IMemberInfo> ReadWriteMembers{
		get{
			// 同上，惰性算一次並緩存（見 _ReadWrite）。
			return _ReadWrite ??= MkSubset(M => M.CanRead && M.CanWrite);
		}
	}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#Descr[
就是 {nameof(Members)} 的 TryGetValue，O(1)（保序字典按鍵查是常數時間），不掃全表。
]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IMemberInfo? M);

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}，訊息含可用名清單。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name);

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

	// ---- 私有輔助（實現見 JsonTypeInfoInfo.Impl.cs）----

	[Doc($"""
#Sum[由成員表過濾出一份子集快照，順序同 {nameof(Members)}。]

#Params([[Pick, 判據；拿成員本體判定是否收進子集]])

#Rtn[新的保序字典；鍵是成員名、值是成員本體]

#Descr[
建一份新的 {nameof(OrderedDictionary<,>)}，依 {nameof(Members)} 的順序把符合判據的成員放進去。
每次都新建一份，不與成員表共用；三份子集屬性會把它緩存下來。
]
""")]
	private partial IDictionary<str, IMemberInfo> MkSubset(Func<IMemberInfo, bool> Pick);
}
