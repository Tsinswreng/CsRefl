namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源的型別元資料：對一個 {nameof(Type)} 建立 {nameof(ITypeInfo)}。]

#Descr[
分類靠介面分析（`{nameof(IDictionary<,>)}` 是字典、`{nameof(IEnumerable<>)}` 是集合、
基本型別是標量、其餘是物件），
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。

實測（`PoUser`）：{nameof(Kind)} 是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
`typeof(List<str>)` 得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
`typeof(Dictionary<str, i32>)` 得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
`typeof(PoColor)`（枚舉）得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}。
]

#Descr[
成員只收公開實例屬性與公開實例字段，順序 = 契約序
（{nameof(TypeInfoSorter)}：基類在前、同類內屬性段在字段段前、段內收集序；
同名遮蔽只留最靠近實例的那份）。

注意屬性與字段跨 table 沒有統一的源碼行號，
源碼裏屬性字段交錯聲明時，同類內一律「屬性在前、字段在後」。

這不是看漏了源碼順序，而是刻意選定的穩定序：
{nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)}
是兩次收集，跨 table 沒有共同序可依。

實測（`PoUser`）：{nameof(Members)} 是 11 項，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`；
其中 `Note`（字段）排在 `Token`（屬性）之後，正是「屬性段先、字段段後」的體現。
]

#Descr[
建構子與 {nameof(MkInst)} 實現見 `ReflTypeInfo.Imrl.cs`。
]
""")]
//TswgNote 搞這個基類有甚麼用? 爲甚麼不直接實現接口?
// 已按此拆：TypeInfoBase 已刪除，兩條來源各自實現 ITypeInfo（另一條見 JsonTypeInfoInfo.cs）。
public partial class ReflTypeInfo:ITypeInfo{
	[Doc($"""
#Sum[反射建立元資料所需的成員種類。]

#Descr[
{nameof(ITypeInfoSrc)}、{nameof(ITypeInfoSrcExtn)} 的 DAM 註解都引用本常量，
改動即全包同步。

實測：`typeof(PoUser)` 的成員表能取到 11 項、無參工廠非 null，
即「公共屬性、公共字段、無參構造函數」這三檔元數據在 NativeAOT 下確實被保留。
]
""")]
	internal const DynamicallyAccessedMemberTypes ReflDam
		= DynamicallyAccessedMemberTypes.Interfaces
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicFields
		| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

	[Doc($$"""
#Sum[對一個型別建立元資料。]

#Params([[Type, 要建立元資料的型別]])

#Descr[
調用方這樣寫：

```csharr
var Info = new ReflTypeInfo(typeof(PoUser));
// 建構子一次做完分類、收集成員、找鍵值型別、建無參工廠。

Info.Kind;                        // JsonTypeInfoKind.Object
Info.Members.Count;               // 11
Info.ElementType;                 // null（物件型別不是集合）
Info.CanMkInst;                   // true

Info.GetMember(nameof(PoUser.Age));   // 之後按名查直接用這份結果，不重算

new ReflTypeInfo(typeof(PoNoCtor)).CanMkInst;   // false：只有帶參構造函數
```

DAM 註解：反射建立元資料需要 接口、公共屬性、公共字段、無參構造函數 的元數據被保留
（AOT 剪裁的前提，見 {{nameof(ReflDam)}} 的說明）。
]
""")]
	public partial ReflTypeInfo(
		Type Type
	);

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
	//TswgNote 爲甚麼要在基類裏放這個東西? 我越來越搞不懂你這個TypeInfoBase到底是甚麼東西了
	//光是搞這個抽象類而不是直接實現接口就已經很讓人迷惑了 你還在基類 裏面搞一堆看不懂的操作
	// 已按此拆：Kind 等型別事實不再放共用基類，兩條來源各自宣告、建構子各自賦值。
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
	//TswgNote 爲甚麼有這麼多脫褲子放屁的東西? 給我個理由?
	// 已按此清掉：構造期算出來的事實直接落在屬性上（自動屬性），不再另存欄位由屬性轉發。
	public Type? KeyType{get;set;}

	// ---- 惰性快取：第一次用到時才建，之後一直用 ----

	//TswgNote 違反命名規範！沒有一處寫得對的
	// 已按此改：_byName → _ByName；其後成員表改成保序字典本體，這份按名索引不再需要，已刪。

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
實測（`PoUser`）：這份表是 9 項，正好等於 {nameof(InstViewDict)} 的鍵表。
]
""")]
	public volatile IDictionary<str, IMemberInfo>? _ReadWrite;

	[Doc($"""
#Sum[無參實例工廠，形狀與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致；null 表示本型別不可建實例。]

#Descr[
構造期由 {nameof(TryBuildMkInst)} 算好、直接落在本屬性上，不再另存欄位轉發。

實測：`typeof(PoUser)` 的這個屬性非 null，調一次得到一個 `PoUser` 實例；
`typeof(PoNoCtor)`（只有帶參構造函數）為 null。
JIT 下它由表達式樹一次編譯成委託；
NativeAOT 下表達式樹不能 Comrile（會拋 {nameof(PlatformNotSupportedException)}），
故退成每次調 {nameof(Activator)}.{nameof(Activator.CreateInstance)}。
]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public Func<obj>? CreateObject{get;set;}

	[Doc($"""
#Sum[反射來源沒有官方 {nameof(JsonTypeInfo)}，預設 null；可賦值。]

#Descr[
預設 null。賦值留給調用方自行取用（例如把別處拿到的官方本體掛上來）。
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
#Sum[建立無參實例；不可建時拋 {nameof(NotSupportedException)}。]

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
實測（`PoUser`）：這裡是 9 項，正好等於 {nameof(InstViewDict)} 的鍵表。
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
	public partial bool TryGetMember(str Name, [NotNullWhen(true)] out IMemberInfo? M);

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

	// ---- 私有輔助（實現見 ReflTypeInfo.Impl.cs）----
	// 參數上的剪裁註解（{nameof(DynamicallyAccessedMembersAttribute)}）只寫在 Impl 側，
	// `partial` 合併時兩邊都標會報 CS0579。

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

	[Doc($"""
#Sum[型別分類。]

#Params([[T, 要分類的型別]])

#Rtn[官方 {nameof(JsonTypeInfoKind)} 分類結果]

#Descr[
先剝 {nameof(Nullable<int>)}，
再按 字典、標量、集合、物件 的優先序判定，
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量對應官方的 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}，
見 {nameof(ITypeInfo.Kind)} 的說明）。

實測取值：

+ `typeof(PoColor)`（枚舉）→ {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}；
+ `typeof(List<str>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
+ `typeof(Dictionary<str, i32>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
+ `typeof(PoUser)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
+ `typeof(DateTime[])` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}
	（集合優先於元素型別的標量性，故不是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。
]
""")]
	private static partial JsonTypeInfoKind ComputeKind(Type T);

	[Doc($"""
#Sum[是否字典。]

#Params([[T, 要判定的型別]])

#Rtn[是字典返回 true]

#Descr[
實現了非泛型 {nameof(System.Collections.IDictionary)}，
或（直接是或實現了）{nameof(IDictionary<,>)}。

實測：`typeof(Dictionary<str, i32>)` 兩條都命中；
`typeof(List<str>)` 兩條都不命中，返回 false。
]
""")]
	private static partial bool IsDictionary(Type T);

	[Doc($"""
#Sum[找集合的元素型別。]

#Params([[T, 要查找的型別]])

#Rtn[元素型別；非集合為 null]

#Descr[
數組取 {nameof(Type.GetElementType)}；
字典取值型別（與官方 {nameof(JsonTypeInfo.ElementType)} 一致，
官方對字典的元素型別就是值型別）；
其餘取 `{nameof(IEnumerable<>)}` 的泛型實參。

實測取值：

+ `typeof(i32[])` → `typeof(i32)`；
+ `typeof(Dictionary<str, i32>)` → `typeof(i32)`（值型別，不是鍵型別）；
+ `typeof(List<str>)` → `typeof(str)`；
+ `typeof(i32)`、`typeof(PoColor)` → null。
]
""")]
	private static partial Type? FindElementType(Type T);

	[Doc($"""
#Sum[找字典的鍵型別。]

#Params([[T, 要查找的型別]])

#Rtn[鍵型別；非字典為 null]

#Descr[
即 `{nameof(IDictionary<,>)}` 的第一個泛型實參。

實測：`typeof(Dictionary<str, i32>)` → `typeof(str)`；
`typeof(List<str>)` 不是字典，返回 null
（此時 {nameof(ElementType)} 有值 `typeof(str)`、本項為 null）。
]
""")]
	private static partial Type? FindKeyType(Type T);

	[Doc($"""
#Sum[找 `T` 上實現了「泛型定義為 `Def`」的最近介面。]

#Params([[T, 要查找的型別], [Def, 泛型介面的開放泛型定義]])

#Rtn[找到的介面型別；沒有為 null]

#Descr[
`T` 本身是該介面也認。

實測：`Def` 傳 `typeof({nameof(IDictionary<,>)})` 時，
`typeof(Dictionary<str, i32>)` 返回它自己實現的 `{nameof(IDictionary<string, int>)}`；
`typeof(List<str>)` 返回 null。

只認第一個匹配的介面，故有多個同定義介面時取 {nameof(Type.GetInterfaces)} 的順序（穩定）。
]
""")]
	private static partial Type? FindGenericIface(
		Type T,
		Type Def
	);

	[Doc($"""
#Sum[收集成員：公開實例屬性（排除索引器）加公開實例字段。]

#Params([[T, 要收集的型別]])

#Rtn[成員表（尚未規整，由建構子統一處理）]

#Descr[
屬性段在前、字段段後；
段內順序 = 收集序（即 {nameof(Type.GetProperties)} 與 {nameof(Type.GetFields)} 交出的順序，
它就是可用的穩定序；
不用 {nameof(MemberInfo.MetadataToken)}：NativeAOT 的 NativeFormat 元數據不提供它，取會拋）。

類型層次的合併（基類在前）與同名遮蔽的去重由 {nameof(TypeInfoSorter)} 統一處理。

實測（`PoUser`）：收到 11 項，屬性段 10 項加字段段 1 項（`Note`）；
同型別上的 `StaticNote`（靜態）、`Hidden`（私有）、`this[i32]`（索引器）都不收。
]
""")]
	private static partial IReadOnlyList<IMemberInfo> CollectMembers(
		Type T
	);

	[Doc($"""
#Sum[建立無參實例委託。]

#Params([[T, 要建實例的型別]])

#Rtn[無參實例工廠；無構造函數、抽象、接口返回 null]

#Descr[
值型別取默認值；類型別解析無參構造函數。

返回型別與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致。

JIT 下用表達式樹一次編譯成委託（靜態引用構造函數，剪裁友好）；
NativeAOT 不支持動態編譯
（{nameof(Expression)}.{nameof(Expression.Lambda)} 的 Comrile 拋
{nameof(PlatformNotSupportedException)}），
退回 {nameof(Activator)}.{nameof(Activator.CreateInstance)}——
構造函數元數據已由 {nameof(ReflDam)} 保證保留。

實測：`typeof(PoUser)` 返回委託，調一次得到 `PoUser` 實例；
`typeof(PoNoCtor)`（只有帶參構造函數）返回 null，故其 {nameof(ITypeInfo.CanMkInst)} 為 false。
]
""")]
	private static partial Func<obj>? TryBuildMkInst(
		Type T
	);
}
