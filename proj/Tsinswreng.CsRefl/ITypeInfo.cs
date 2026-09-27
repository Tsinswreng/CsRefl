namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料門面（按名）：型別、官方本體、名清單、按名取型別、按名能力、實例工廠。]

#Descr[
門面的語言是「名字、型別、值、能力」——調用方不需要知道裏面是哪一套實現，
也不需要碰官方的成員物件（那些是兩套來源的內部材料）。

兩套來源各自實現本接口，對外語義一致：
{nameof(ReflTypeInfoSrc)} 走反射、{nameof(JsonTypeInfoSrc)} 走官方源生成。

實測（`PoUser`，兩套來源逐項相同）：
{nameof(Kind)} 都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
{nameof(ReadableMembers)}、{nameof(WritableMembers)}、{nameof(ReadWriteMembers)} 的項數分別是 10、10、9。
]

#Descr[
官方已有的概念一律直接拿來用：
{nameof(Kind)} 就是官方 {nameof(JsonTypeInfoKind)}、
{nameof(ElementType)}／{nameof(KeyType)} 與官方同名同義、
{nameof(CreateObject)} 就是官方那條委託的形狀、
{nameof(Json)} 直接把官方 {nameof(JsonTypeInfo)} 本體交出去。

成員層不以 {nameof(System.Object)} 交出官方成員物件：
要成員型別走 {nameof(TryGetMemberType)}、要能力走 {nameof(CanRead)}／{nameof(CanWrite)}、
要值走 {nameof(ITypeInfoExtn.TryGet)}／{nameof(ITypeInfoExtn.TrySet)}。
]

#Descr[
事實成員皆可賦值：賦值＝換掉那件事實（不合併、不拷貝、不驗證），留給調用方自行取用。
由別的成員算出來的值只讀：{nameof(CanMkInst)} 就是 {nameof(CreateObject)} 是否為 null，
{nameof(ReadableMembers)}／{nameof(WritableMembers)}／{nameof(ReadWriteMembers)} 是 {nameof(Members)} 過濾出來的。

換了 {nameof(Members)} 不會重建按名索引與名清單快取，故換整張成員表通常該重建一份實例。
]
""")]
public partial interface ITypeInfo{
	[Doc($"""
#Sum[本元資料對應的型別。]

#Descr[
實測：兩套來源查 `PoUser` 得到的這個屬性都是 `typeof(PoUser)`（官方 {nameof(JsonTypeInfo.Type)} 同值）。
]
""")]
	Type Type{get;set;}

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
	JsonTypeInfoKind Kind{get;set;}

	[Doc($"""
#Sum[官方 JSON 型別元資料本體；反射側為 null。]

#Descr[
要官方能力直接從這裡拿，本包不另做轉譯：
{nameof(JsonTypeInfo.CreateObject)}、{nameof(JsonTypeInfo.Properties)}、
{nameof(JsonTypeInfo.Converter)}、{nameof(JsonTypeInfo.UnmappedMemberHandling)}。

實測：從 {nameof(JsonTypeInfoSrc)} 查 `PoUser` 時非 null 且
`{nameof(Json)}.{nameof(JsonTypeInfo.Type)}` 是 `typeof(PoUser)`；從 {nameof(ReflTypeInfoSrc)} 查同型別時為 null。
]
""")]
	JsonTypeInfo? Json{get;set;}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.ElementType)} 同義。

實測：`typeof(List<str>)` → `typeof(str)`；`typeof(Dictionary<str, i32>)` → `typeof(i32)`（字典取值型別）；`typeof(PoUser)` → null。
]
""")]
	Type? ElementType{get;set;}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.KeyType)} 同義。

實測：`typeof(Dictionary<str, i32>)` → `typeof(str)`；`typeof(List<str>)` → null。
]
""")]
	Type? KeyType{get;set;}

	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	// 註：本批註原文保留。它原本掛在 Members 上，而 Members 已按新方向
	//（門面只按名、不外露官方成員物件）移除，處置待你確認。

	[Doc($$"""
#Sum[可讀成員；鍵是成員名、值是成員本體，順序即契約序（基類在前、同類內宣告序）。]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

foreach(var (Name, M) in Info.ReadableMembers){
	// 依次拿到 Id、Name、Age、Email、Married、Tags、Extra、Secret、Level、Note（10 個）
}

Info.ReadableMembers.ContainsKey(nameof(PoUser.Secret));       // true：只讀成員算「可讀」
Info.ReadableMembers[nameof(PoUser.Age)].PropertyType;          // typeof(i32)：值就是成員本體
```

按實例緩存：第一次由成員表現算一份，之後每次返回同一份表實例。

這三份子集都是本庫持有的那一份：交出同一個實例，不另做副本；
要自己改就先複製一份，改了本庫那一份會影響之後讀到它的人。
]
""")]
	IDictionary<str, IMemberInfo> ReadableMembers{get;}

	[Doc($$"""
#Sum[可寫成員；鍵是成員名、值是成員本體，順序同 {nameof(ReadableMembers)}。]

#Descr[
調用方這樣寫（拼 SQL 列或表單欄位時就吃這個順序）：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.WritableMembers.Count;                                    // 9
Info.WritableMembers.ContainsKey(nameof(PoUser.Secret));       // false：只讀成員不可寫
Info.WritableMembers.ContainsKey(nameof(PoUser.Token));        // true：只寫成員算「可寫」
```
]
""")]
	IDictionary<str, IMemberInfo> WritableMembers{get;}

	[Doc($$"""
#Sum[既可讀又可寫的成員；鍵是成員名、值是成員本體，順序同 {nameof(ReadableMembers)}。]

#Descr[
列與表單欄位就是這一批：取得到、也寫得回。

調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.ReadWriteMembers.Count;                                   // 9
Info.ReadWriteMembers.ContainsKey(nameof(PoUser.Secret));      // false：只讀
Info.ReadWriteMembers.ContainsKey(nameof(PoUser.Token));       // false：只寫
```

這也是 {nameof(InstDict)} 的鍵表口徑：它只把可讀且可寫的成員當鍵。
]
""")]
	IDictionary<str, IMemberInfo> ReadWriteMembers{get;}

	[Doc($$"""
#Sum[按名取成員的宣告型別；成員不存在返回 false。]

#Params([[Name, 成員名], [T, 宣告型別；失敗時為 null]])

#Rtn[成員存在返回 true]

#Descr[
這是「成員型別」在門面上的入口（CsSql 要列型別時用它），調用方不必碰官方成員物件。

調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.TryGetMemberType(nameof(PoUser.Age), out var T1);    // true；T1 是 typeof(i32)
Info.TryGetMemberType(nameof(PoUser.Note), out var T2);   // true；T2 是 typeof(str)（Note 是字段）
Info.TryGetMemberType(nameof(PoUser.Tags), out var T3);   // true；T3 是 typeof(List<str>)
Info.TryGetMemberType("NoSuch", out _);                   // false
```
]
""")]
	bool TryGetMemberType(str Name, out Type? T);

	[Doc($$"""
#Sum[按名問「這個成員能不能讀」。]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.CanRead(nameof(PoUser.Age));      // true
Info.CanRead(nameof(PoUser.Secret));   // true（只讀成員讀得到）
Info.CanRead(nameof(PoUser.Token));    // false（只寫成員讀不到）
Info.CanRead("NoSuch");                // false
```
]
""")]
	bool CanRead(str Name);

	[Doc($$"""
#Sum[按名問「這個成員能不能寫」。]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.CanWrite(nameof(PoUser.Age));      // true
Info.CanWrite(nameof(PoUser.Secret));   // false（只讀）
Info.CanWrite(nameof(PoUser.Token));    // true（只寫成員寫得進）
Info.CanWrite("NoSuch");                // false
```
]
""")]
	bool CanWrite(str Name);

	[Doc($"""
#Sum[無參實例工廠；形狀與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致，沒有工廠時為 null。]

#Descr[
實測：`typeof(PoUser)` 兩套來源都非 null，調一次得到一個 `PoUser` 實例；
`typeof(PoNoCtor)`（只有帶參構造函數）兩套來源都是 null。
]
""")]
	Func<obj>? CreateObject{get;set;}

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
實測：`{nameof(MkInst)}()` 得到一個 `PoUser` 新實例；對 `typeof(PoNoCtor)` 拋 {nameof(NotSupportedException)}。
先查 {nameof(CanMkInst)} 可以避免這個異常。
]
""")]
	obj? MkInst();

	[Doc($$"""
#Sum[全部成員；鍵是成員名、值是成員本體，順序即契約序（基類在前、同類內宣告序）。]

#Descr[
值是成員契約 {nameof(IMemberInfo)}：反射側由 {nameof(ReflMemberInfo)} 包官方 {nameof(MemberInfo)}、
Json 側由 {nameof(JsonMemberInfo)} 包官方 {nameof(JsonPropertyInfo)}。

調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

Info.Members.Count;                              // 11
Info.Members[nameof(PoUser.Age)].PropertyType;    // typeof(i32)：按名直接取成員，O(1)
Info.Members.Keys;                               // 成員名，順序同契約序
```

實現用保序字典，故枚舉順序等於插入順序，即契約序；按名查也就是它的 TryGetValue。

可賦值：換整張成員表；賦值不重建三份子集快取，故換表通常該重建一份實例。
]
""")]
	IDictionary<str, IMemberInfo> Members{get;set;}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#Descr[
實測：`TryGetMember("Age", out var M)` 返回 true；`"NoSuch"` 返回 false 且 `M` 為 null（不拋）。
]
""")]
	bool TryGetMember(str Name, [NotNullWhen(true)] out IMemberInfo? M);

	[Doc($"""
#Sum[按名取成員；取不到拋 {nameof(KeyNotFoundException)}。]

#Descr[
實測：`GetMember("Age")` 與 `TryGetMember("Age", out var M)` 命中時返回同一實例。
]
""")]
	IMemberInfo GetMember(str Name);
}







