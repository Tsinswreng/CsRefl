namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料門面：{nameof(Type)}、官方型別元資料、成員表、集合鍵值型別、實例工廠、按名查詢。]

#Descr[
兩套來源各自實現本接口，對外語義一致，
調用方不需要知道自己拿到的是哪一套：
{nameof(ReflTypeInfoSrc)} 走反射，
{nameof(JsonTypeInfoSrc)} 走官方源生成元資料。

實測（`PoUser`，兩套來源逐項相同）：
{nameof(Kind)} 都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
{nameof(Members)} 都是 11 項，順序都是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`。
]

#Descr[
設計原則：官方已有的概念一律直接交官方物件出去，本包不重複暴露一遍。

+ {nameof(Json)} 交官方 {nameof(JsonTypeInfo)} 本體（反射側為 null）——
	官方的 {nameof(JsonTypeInfo.CreateObject)}、{nameof(JsonTypeInfo.Properties)}、
	{nameof(JsonTypeInfo.UnmappedMemberHandling)} 等一律從那裡拿，本接口不再照抄。
+ {nameof(Members)} 的元素是官方成員物件：反射側是官方 {nameof(MemberInfo)}
	（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}），Json 側是官方 {nameof(JsonPropertyInfo)}。
+ {nameof(Kind)} 就是官方 {nameof(JsonTypeInfoKind)}；
	{nameof(ElementType)} 與 {nameof(KeyType)} 與官方同名同義。
+ {nameof(CreateObject)} 就是官方那條委託的形狀。
]

#Descr[
自研面只剩「以字符串為鍵」這一件事，它按「要不要按實例緩存」分成兩處：

+ 按名查（{nameof(TryGetMember)}／{nameof(GetMember)}）與名清單
	（{nameof(ReadableNames)}／{nameof(WritableNames)}）留在本接口上：
	按名查必須 O(1)、名清單只該算一次，兩者都要按實例緩存，
	放進擴展方法就只剩每次重算一條路（見 {nameof(TryGetMember)} 的說明）。
+ 按名讀寫的順手寫法（{nameof(ITypeInfoExtn.TryGet)}／{nameof(ITypeInfoExtn.TrySet)}）
	落在 {nameof(ITypeInfoExtn)} 上：那只是「查成員 + 讀值」兩步的合寫，沒有狀態可緩存。

成員自身的操作（取名字、宣告型別、可讀可寫、讀值寫值）一律在 {nameof(MemberExtn)} 上，
不在本接口重複一遍。
]
""")]
public partial interface ITypeInfo{
	[Doc($"""
#Sum[本元資料對應的型別。]

#Descr[
實測：兩套來源查 `PoUser` 得到的這個屬性都是 `typeof(PoUser)`，
與 `{nameof(ReferenceEquals)}` 比較為 true（官方 {nameof(JsonTypeInfo.Type)} 同值）。
]
""")]
	Type Type{get;}

	[Doc($"""
#Sum[型別分類，直接用官方 {nameof(JsonTypeInfoKind)}。]

#Descr[
實測取值（兩套來源一致）：

+ `typeof(PoColor)`（枚舉）→ {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}；
+ `typeof(List<str>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
+ `typeof(Dictionary<str, i32>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
+ `typeof(PoUser)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}。
]
""")]
	JsonTypeInfoKind Kind{get;}

	[Doc($"""
#Sum[全部成員，順序即契約序：基類在前、同類內按來源的宣告序。]

#Descr[
元素是官方成員物件，本包不包裝、不轉發其成員：
反射側是官方 {nameof(MemberInfo)}（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}），
Json 側是官方 {nameof(JsonPropertyInfo)}。
故反射獨有的 {nameof(MemberInfo.MemberType)} 與 Json 獨有的
{nameof(JsonPropertyInfo.Get)} 都在元素本身上，直接取即可。

順序由 {nameof(TypeInfoSorter)} 統一規整，故兩套來源給出同一個順序。

實測（`PoUser`，兩套來源逐位相同）：

+ 共 11 項，依次是
	`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`；
+ 反射側首項是 `Id` 的 {nameof(PropertyInfo)}、末項是 `Note` 的 {nameof(FieldInfo)}
	（帶 `[JsonInclude]` 的字段）；
+ Json 側首項是 `Id` 的 {nameof(JsonPropertyInfo)}、末項是 `Note` 的 {nameof(JsonPropertyInfo)}；
+ 靜態成員 `StaticNote`、私有成員 `Hidden`、索引器 `this[i32]` 兩側都不在其中。

同名遮蔽已去重：子類用 `new` 遮蔽基類同名成員時只留離實例最近的那份宣告，並佔原位置。
]

#Descr[
為甚麼是列表而不是字典：成員表本身就是一段序列，官方的兩側也都是序列——
反射側是 {nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)}
交出的數組，Json 側是官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.Properties)}
（{nameof(IList<JsonPropertyInfo>)}）。
本包不重造官方已有的東西，{nameof(Members)} 就照這個形狀對齊。

按名查不靠掃這張表：走 {nameof(TryGetMember)}，它由實現維護惰性索引，是 O(1)。
]
""")]
	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	IReadOnlyList<obj?> Members{get;}

	[Doc($"""
#Sum[官方 JSON 型別元資料本體；反射側為 null。]

#Descr[
要官方能力直接從這裡拿，本包不另做轉譯：
{nameof(JsonTypeInfo.CreateObject)}、{nameof(JsonTypeInfo.Properties)}、
{nameof(JsonTypeInfo.Converter)}、{nameof(JsonTypeInfo.UnmappedMemberHandling)}、
{nameof(JsonTypeInfo.NumberHandling)}、{nameof(JsonTypeInfo.PolymorphismOptions)}。

實測：從 {nameof(JsonTypeInfoSrc)} 查 `PoUser` 時非 null 且
`{nameof(Json)}.{nameof(JsonTypeInfo.Type)}` 是 `typeof(PoUser)`；
從 {nameof(ReflTypeInfoSrc)} 查同型別時為 null。
]
""")]
	JsonTypeInfo? Json{get;}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.ElementType)} 同義。

實測取值（兩套來源一致）：

+ `typeof(List<str>)` → `typeof(str)`；`typeof(i32[])` → `typeof(i32)`；
+ `typeof(Dictionary<str, i32>)` → `typeof(i32)`（字典取的是值型別，不是鍵型別）；
+ `typeof(i32)`、`typeof(PoUser)` → null。
]
""")]
	Type? ElementType{get;}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.KeyType)} 同義。

實測取值（兩套來源一致）：

+ `typeof(Dictionary<str, i32>)` → `typeof(str)`；
+ `typeof(List<str>)` → null（不是字典）。
]
""")]
	Type? KeyType{get;}

	[Doc($"""
#Sum[無參實例工廠；形狀與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致；沒有工廠時為 null。]

#Descr[
官方兩側都有這條：Json 側直接就是官方 {nameof(JsonTypeInfo.CreateObject)}，
反射側由 {nameof(ReflTypeInfo)} 算出來（JIT 下編譯成委託、NativeAOT 下退成反射創建）。

實測：`typeof(PoUser)` 兩套來源都非 null，調一次得到一個 `PoUser` 實例；
`typeof(PoNoCtor)`（只有帶參構造函數）兩套來源都是 null。
]
""")]
	Func<obj>? CreateObject{get;}

	[Doc($"""
#Sum[本型別能否建立無參實例；判據就是 {nameof(CreateObject)} 是否為 null。]

#Descr[
實測：`typeof(PoUser)` 兩套來源都是 true；`typeof(PoNoCtor)` 兩套來源都是 false。
]
""")]
	bool CanMkInst{get;}

	[Doc($"""
#Sum[建立一個無參實例；不可建時拋 {nameof(NotSupportedException)}。]

#Rtn[新實例]

#Descr[
實測：`{nameof(MkInst)}()` 得到一個 `PoUser` 新實例，其 `Id` 可立即賦值；
對 `typeof(PoNoCtor)` 拋 {nameof(NotSupportedException)}，訊息含型別全名。
先查 {nameof(CanMkInst)} 可以避免這個異常。
]
""")]
	obj? MkInst();

	[Doc($"""
#Sum[可讀成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）。

按實例緩存：第一次由 {nameof(Members)} 現算一份，之後每次返回同一份清單實例，
故在循環裏反復讀不會反復計算。

實測（`PoUser`）：10 個名，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Note`
（只讀的 `Secret` 在、只寫的 `Token` 不在，因為判據是「能不能讀」）。

用途：要把物件序列化成一行 SQL 或一份前端表單時直接遍歷它，不必自己過濾成員。
]
""")]
	IReadOnlyCollection<str> ReadableNames{get;}

	[Doc($"""
#Sum[可寫成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）；同樣按實例緩存，第二次起返回同一份清單實例。

實測（`PoUser`）：10 個名，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Token`、`Note`
（只寫的 `Token` 在、只讀的 `Secret` 不在）。

用途：要按外部字典回填物件時，先拿這個清單擋掉不該寫的鍵。
]
""")]
	IReadOnlyCollection<str> WritableNames{get;}

	[Doc($"""
#Sum[按名（成員的官方名字）查成員。]

#Params([[Name, 成員名], [M, 查到的官方成員物件；未命中為 null]])

#Rtn[命中返回 true；未命中或名字為 null 返回 false]

#Descr[
命中的 {nameof(M)} 是官方成員物件：反射側是 {nameof(MemberInfo)}、Json 側是 {nameof(JsonPropertyInfo)}；
兩側取名字一律走 {nameof(MemberExtn.Name)}，不必自己分辨型別。

名字比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)}
（成員名是程式碼識別符，不該受當前文化影響）。

#strong[本查詢必須 O(1)。] 實現要在第一次按名查時惰性建一份 {nameof(Dictionary<,>)} 索引，
之後每次查都走索引；不得每次調用都線性掃 {nameof(Members)}。

實測（`PoUser`，兩套來源一致）：

+ `{nameof(TryGetMember)}("Age", out var M)` 返回 true，
	`{nameof(MemberExtn.Name)}(M)` 是 "Age"、
	`{nameof(MemberExtn.DeclaringType)}(M)` 是 `typeof(PoUser)`、
	`{nameof(MemberExtn.CanRead)}(M)` 與 `{nameof(MemberExtn.CanWrite)}(M)` 都是 true；
+ `"NoSuch"` 返回 false 且 `M` 為 null（不拋）；
+ `"Token"`（只寫）返回 true 但 `{nameof(MemberExtn.CanRead)}(M)` 為 false。

名字傳 null 返回 false 而不拋，故適合接外部傳來的名字。
]
""")]
	bool TryGetMember(str Name, [NotNullWhen(true)] out obj? M);

	[Doc($"""
#Sum[按名取成員；取不到就拋。]

#Params([[Name, 成員名]])

#Rtn[命中的官方成員物件]

#Descr[
與 {nameof(TryGetMember)} 成對：未命中時拋 {nameof(KeyNotFoundException)}，
訊息含可用成員清單（實測含 "Age" 這個子串可被斷言），故拼錯名字時不必自己去列成員。

實測：`{nameof(GetMember)}("Age")` 與 `{nameof(TryGetMember)}("Age", out var M)` 命中時
返回的是同一實例（{nameof(ReferenceEquals)} 為 true），即共用同一份按名索引。
]
""")]
	obj? GetMember(str Name);
}
