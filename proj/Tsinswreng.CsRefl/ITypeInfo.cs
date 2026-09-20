namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料門面：{nameof(Type)}、官方型別元資料、成員表、集合鍵值型別。]

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

官方沒有的只有「以字符串為鍵」這一件事，它落在 {nameof(ITypeInfoExtn)} 的擴展方法上
（按名查成員、按名讀寫、名清單、無參實例），不佔本接口成員。
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
""")]
	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	//你上面Doc 也沒用nameof語法我想f12跳轉都跳不了
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
}