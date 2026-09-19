namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(IMemberInfo)} 是型別上單個可訪問成員的元資料，是門面的最小單元。]

#Descr[
與官方 API 的關係（先講清楚官方到底有甚麼，免得再按錯的印象設計）：

+ 官方*沒有*共同的成員基類。
	反射側是 {nameof(System.Reflection.MemberInfo)}
	（{nameof(PropertyInfo)} 與 {nameof(FieldInfo)} 的基類），
	JsonTypeInfo 側是 {nameof(JsonPropertyInfo)}，
	而 {nameof(JsonPropertyInfo)} 是獨立抽象類、*不繼承* {nameof(System.Reflection.MemberInfo)}
	（已用編譯器核實）。
+ 官方兩側*共有*的是 {nameof(ICustomAttributeProvider)}：
	{nameof(System.Reflection.MemberInfo)} 實現它，
	{nameof(JsonPropertyInfo)} 也提供 {nameof(JsonPropertyInfo.AttributeProvider)} 給它。
	故門面的特性出口就用這一個官方接口，
	不再自造 `Attrs` 與 `TryGetAttr`。
+ 官方已有的名字與型別一律沿用：
	{nameof(Name)}、{nameof(PropertyType)}、{nameof(DeclaringType)}、
	{nameof(MemberTypes)}、{nameof(Get)} 與 {nameof(Set)}
	（型別與名字照官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)} 與
	{nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)}）、
	{nameof(ITypeInfo.CreateObject)}（照官方 {nameof(JsonTypeInfo.CreateObject)}）。
+ 官方沒有的概念才自研，且只有一件：*以字符串為鍵*。
	官方兩側都沒有「按名取成員、按名讀寫」的抽象，
	故本接口補上按名讀寫（{nameof(TryGet)} 與 {nameof(TrySet)}）與
	型別層的按名查詢（{nameof(ITypeInfo.TryGetMember)}）。
]

#Descr[
同一份成員元資料在兩套來源下的樣子，實測對照（成員取 `PoUser` 的 `Age` 與 `Note`）：

+ `Age`（公開讀寫屬性）：
	反射源 {nameof(Member)} 是 {nameof(PropertyInfo)}（`Member is {nameof(PropertyInfo)}` 為 true）、
	{nameof(Json)} 為 null、{nameof(AttributeProvider)} 就是那個 {nameof(PropertyInfo)} 本身、
	{nameof(MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}；
	Json 源 {nameof(Json)} 是 {nameof(JsonPropertyInfo)}、{nameof(Member)} 為 null、
	{nameof(Get)} 與 {nameof(Set)} 就是官方委託。
+ `Note`（帶 `[JsonInclude]` 的公開字段）：
	反射源的 {nameof(MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
	Json 源只能報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。
+ 兩側的 {nameof(Name)} 都是 "Age"、{nameof(PropertyType)} 都是 `typeof(i32)`、
	{nameof(DeclaringType)} 都是 `typeof(PoUser)`、{nameof(Get)} 與 {nameof(Set)} 都非 null。
]

#Descr[
語義約定：

+ {nameof(Name)} 是「鍵」：CsSql 用它當列名，物件與字典互轉用它當字典鍵。
	反射源的 {nameof(Name)} 是 C# 成員名；
	JsonTypeInfo 源的 {nameof(Name)} 是官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Name)}（JSON 名）。
	「用 {nameof(JsonTypeInfoSrc)} 當來源」的成立前提是 JSON 名等於 C# 名
	（由調用方保證命名策略為 null 且無 `[JsonPropertyName]`）。
+ 非公開成員、靜態成員、索引器不進門面（兩套來源一致）。
	實測：`PoUser` 上的私有屬性、`static` 屬性、`this[i32]` 索引器都不出現在
	{nameof(ITypeInfo.Members)} 裏。
+ 讀寫動作的「前置檢查」：
	實例型別不符或成員不可讀寫時，{nameof(TryGet)} 與 {nameof(TrySet)} 返回 false；
	值本身型別不符的異常照常拋出，不吞（那是調用方的 bug，不是查詢失敗）。
]
""")]
public interface IMemberInfo{
	[Doc($"""
#Sum[官方反射成員本體（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}）；{nameof(JsonTypeInfoSrc)} 來源為 null。]

#Descr[
實測：查 `PoUser` 的 `Age`，反射源這個屬性是一個 {nameof(PropertyInfo)}
（`Member is {nameof(PropertyInfo)}` 為 true）、Json 源為 null；
查 `Note` 時反射源這個屬性是一個 {nameof(FieldInfo)}。
]
""")]
	global::System.Reflection.MemberInfo? Member{get;}

	[Doc($"""
#Sum[官方 JSON 成員本體（{nameof(JsonPropertyInfo)}）；反射來源為 null。]

#Descr[
實測：查 `PoUser` 的 `Age`，Json 源這個屬性非 null，且
`M.{nameof(Json)}!.{nameof(JsonPropertyInfo.Name)}` 是 "Age"；
反射源的同一個成員這裡是 null（`JsonPropertyInfo` 不是 `MemberInfo` 的子類）。
]
""")]
	JsonPropertyInfo? Json{get;}

	[Doc($"""
#Sum[成員的官方元資料種類（官方 {nameof(MemberTypes)}）。]

#Descr[
反射源轉發官方 {nameof(System.Reflection.MemberInfo)}.{nameof(System.Reflection.MemberInfo.MemberType)}；
Json 源恆為 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。

實測：查 `PoUser` 的 `Age`，兩套來源都是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}；
查 `Note`（`[JsonInclude]` 字段），反射源是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}，
Json 源只能報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}
（官方 {nameof(JsonPropertyInfo)} 不暴露 `IsProperty`，分不出字段）。

故要精確判斷「是不是字段」時，在反射源上看 {nameof(Member)} 更可靠。
]
""")]
	MemberTypes MemberType{get;}

	[Doc($"""
#Sum[成員名，即對外查詢、讀寫時使用的鍵。]

#Descr[
同一個型別元資料內唯一
（同名遮蔽已由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 去重，
見 {nameof(ITypeInfo.Members)}）。

反射源 = 官方 {nameof(System.Reflection.MemberInfo.Name)}；
JsonTypeInfo 源 = 官方 {nameof(JsonPropertyInfo.Name)}。

實測：兩套來源查 `PoUser` 的成員名都是 "Age"、`Note` 都是 "Note"；
{nameof(ITypeInfo.GetMember)}("Age") 用的字符串就是這個屬性的值，
它同時是 CsSql 的列名與物件轉字典時的字典鍵。
]
""")]
	str Name{get;}

	[Doc($"""
#Sum[成員的型別。]

#Descr[
即屬性的 {nameof(PropertyInfo.PropertyType)} 或字段的 {nameof(FieldInfo.FieldType)}，
與官方 {nameof(JsonPropertyInfo.PropertyType)} 同義。

實測（`PoUser`）：`Age` 是 `typeof(i32)`、`Secret` 是 `typeof(str)`、
`Tags` 是 `typeof(List<str>)`（集合成員給的是集合型別本身，不是 `str`，
元素型別要看 {nameof(ITypeInfo.ElementType)}）。
]
""")]
	Type PropertyType{get;}

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
反射源轉發官方 {nameof(System.Reflection.MemberInfo)}.{nameof(System.Reflection.MemberInfo.DeclaringType)}；
Json 源轉發官方 {nameof(JsonPropertyInfo.DeclaringType)}。

繼承成員的 {nameof(DeclaringType)} 是基類，與實例的執行期型別不同。

實測（`PoUser` 繼承 `PoUserBase`）：`Id` 與 `Name` 的 {nameof(DeclaringType)}
是 `typeof(PoUserBase)`，`Age` 的是 `typeof(PoUser)`；
故要拿實例的執行期型別應查 {nameof(ITypeInfo.Type)}，不是本屬性。
]
""")]
	Type? DeclaringType{get;}

	[Doc($"""
#Sum[本成員可讀（可作為字典值來源、可被 {nameof(TryGet)} 讀取）。]

#Descr[
判據是官方 {nameof(Get)} 是否為 null
（官方 {nameof(JsonPropertyInfo)} 就是這樣表示可讀）；
反射側同樣以「有沒有公開訪問器」得出，故兩套來源口徑一致。

實測（`PoUser`）：`Age` 為 true；只讀的 `Secret` 為 true（它可讀）且其 {nameof(CanWrite)} 為 false；
只寫的 `Token` 為 false（`Token` 的 `get` 是私有的，故對外不可讀）。
]
""")]
	bool CanRead{get;}

	[Doc($"""
#Sum[本成員可寫（可被 {nameof(TrySet)} 寫入）。]

#Descr[
判據同 {nameof(CanRead)}，取官方 {nameof(Set)} 是否為 null。

實測（`PoUser`）：`Age` 為 true；只讀的 `Secret` 為 false（其 {nameof(Set)} 為 null）；
只寫的 `Token` 為 true（可寫但不可讀）。
]
""")]
	bool CanWrite{get;}

	[Doc($"""
#Sum[官方讀值委託，型別與名字照官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)}。]

#Descr[
反射側由 {nameof(PropertyInfo.GetValue)} 與 {nameof(FieldInfo.GetValue)} 建成同一形狀。

不可讀為 null；委託只做取值，前置檢查由 {nameof(TryGet)} 承擔。

實測（`PoUser`，`Age = 30`）：`Age` 非 null，`M.{nameof(Get)}!(User)` 直接讀出 boxed 的 `i32` 30；
只寫的 `Token` 為 null；
Json 源的這個委託就是官方源生成的委託，故 AOT 下讀值不走反射。
]
""")]
	Func<obj, obj?>? Get{get;}

	[Doc($"""
#Sum[官方寫值委託，型別與名字照官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)}。]

#Descr[
不可寫為 null。

實測（`PoUser`）：`Age` 非 null，`M.{nameof(Set)}!(User, 31)` 之後 `User.Age` 是 31；
只讀的 `Secret` 為 null，故 {nameof(TrySet)} 在此之前就返回 false、不會撞到異常。
]
""")]
	Action<obj, obj?>? Set{get;}

	[Doc($"""
#Sum[官方特性提供者（兩側共有的那個官方接口）。]

#Descr[
反射源即成員自身；
Json 源取官方 {nameof(JsonPropertyInfo.AttributeProvider)}。

實測：`PoUser` 的 `Level` 上標了 `[MyDemoAttr("優等級", 2)]`，
兩套來源的這個屬性都非 null，且
{nameof(IMemberInfoExtn.GetCustomAttribute)} 都取回 1 個該特性
（源生成下也取得到，故 AOT 下特性查詢不必另走反射）。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;}

	[Doc($"""
#Sum[讀取實例上的本成員。]

#Params([[O, 實例；null 或型別不符時返回 false], [R, 讀出的值；失敗時為 default]])

#Rtn[實例型別不符或本成員不可讀時返回 false]

#Descr[
實測（`PoUser`，`Age = 30`、`Name = "小明"`）：

+ `Age` 的成員 `{nameof(TryGet)}(User, out var V)` 返回 true，V 是 boxed 的 `i32` 30；
+ `Email`（可空未賦值）返回 true 且 V 是 null；
+ `Secret`（只讀）返回 true 且 V 是 "s"；
+ `Token`（只寫）返回 false（不可讀）；
+ `{nameof(TryGet)}(null, out _)` 返回 false；
+ `{nameof(TryGet)}(new PoColor(), out _)` 返回 false（實例型別不符）。
]
""")]
	bool TryGet(obj? O, out obj? R);

	[Doc($"""
#Sum[寫入實例上的本成員。]

#Params([[O, 實例；null 或型別不符時返回 false], [V, 要寫入的值]])

#Rtn[實例型別不符或本成員不可寫時返回 false]

#Descr[
實測（`PoUser`）：

+ `Age` 的成員 `{nameof(TrySet)}(User, 31)` 返回 true，之後 `User.Age` 是 31；
+ 繼承成員 `Name` 也寫得動：`{nameof(TrySet)}(User, "阿強")` 之後 `User.Name` 是 "阿強"；
+ `Secret`（只讀）返回 false 且不改動實例；
+ 值型別不符（拿 `str` 當 `i32` 寫）不返回 false，而是照常拋異常，
	因為那是調用方的 bug，不是「這個成員不可寫」。
]
""")]
	bool TrySet(obj? O, obj? V);
}