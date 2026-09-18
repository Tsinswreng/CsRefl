namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[型別上單個可訪問成員的元資料，是門面的最小單元。]

#Descr[
與官方 API 的關係（先講清楚官方到底有甚麼，免得再按錯的印象設計）：

+ 官方*沒有*共同的成員基類。
	反射側是 `System.Reflection.MemberInfo`（`PropertyInfo`／`FieldInfo` 的基類），
	JsonTypeInfo 側是 `JsonPropertyInfo`，
	而 `JsonPropertyInfo` 是獨立抽象類、*不繼承* `MemberInfo`（已用編譯器核實）。
+ 官方兩側*共有*的是 `System.Reflection.ICustomAttributeProvider`：
	`MemberInfo` 實現它，
	`JsonPropertyInfo` 也提供 `AttributeProvider` 給它。
	故門面的特性出口就用這一個官方接口，
	不再自造 `Attrs`／`TryGetAttr`。
+ 官方已有的名字與型別一律沿用：
	成員名 `Name`、成員型別 `PropertyType`、宣告型別 `DeclaringType`、
	成員種類 `MemberTypes`、讀寫委託 `Get`／`Set`
	（型別與名字照官方 `JsonPropertyInfo.Get`／`Set`）、
	無參工廠 `CreateObject`（照官方 `JsonTypeInfo.CreateObject`）。
+ 官方沒有的概念才自研，且只有一件：*以字符串為鍵*。
	官方兩側都沒有「按名取成員／按名讀寫」的抽象，
	故本接口補上按名讀寫（`TryGet`／`TrySet`）與
	型別層的按名查詢（`ITypeInfo.TryGetMember`）。
]

#Descr[
語義約定：

+ `Name` 是「鍵」：CsSql 用它當列名，物件↔字典互轉用它當字典鍵。
	反射源的 `Name` 是 C# 成員名；
	JsonTypeInfo 源的 `Name` 是官方 `JsonPropertyInfo.Name`（JSON 名）——
	「用 JsonTypeInfo 當來源」的成立前提是 JSON 名等於 C# 名
	（由調用方保證命名策略為 null 且無 `[JsonPropertyName]`）。
+ 非公開成員、靜態成員、索引器不進門面（兩套來源一致）。
+ 讀寫動作的「前置檢查」：
	實例型別不符、不可讀/不可寫時 `TryGet`/`TrySet` 返回 false；
	值本身型別不符的異常照常拋出，不吞（那是調用方的 bug，不是查詢失敗）。
]
""")]
public interface IMemberInfo{
	[Doc("""
#Sum[官方反射成員本體（`PropertyInfo`／`FieldInfo`）；JsonTypeInfo 來源為 null。]

#Descr[
要官方反射能力（`GetCustomAttributes`、`IsDefined`、`ReflectedType` 等）從這裡拿。
]
""")]
	global::System.Reflection.MemberInfo? Member{get;}

	[Doc("""
#Sum[官方 JSON 成員本體（`JsonPropertyInfo`）；反射來源為 null。]

#Descr[
要官方序列化能力（`IsRequired`、`IsExtensionData`、`NumberHandling` 等）從這裡拿。
]
""")]
	JsonPropertyInfo? Json{get;}

	[Doc("""
#Sum[成員的官方元資料種類（官方 `MemberTypes`）。]

#Descr[
反射源轉發官方 `MemberInfo.MemberType`；
Json 源恆為 `Property`——
官方 `JsonPropertyInfo` 不暴露 `IsProperty`，
故分不出源生成收進來的字段。
]
""")]
	MemberTypes MemberType{get;}

	[Doc("""
#Sum[成員名，即對外查詢、讀寫時使用的鍵。]

#Descr[
同一個型別元資料內唯一
（同名遮蔽已由 `TypeInfoSorter.SortEtDedup` 去重，見 `ITypeInfo.Members`）。

反射源 = 官方 `MemberInfo.Name`；
JsonTypeInfo 源 = 官方 `JsonPropertyInfo.Name`。
]
""")]
	str Name{get;}

	[Doc("""
#Sum[成員的宣告型別。]

#Descr[
即屬性的 `PropertyType`／字段的 `FieldType`，
兩者在官方 `JsonPropertyInfo.PropertyType` 上是同一個概念。
]
""")]
	Type PropertyType{get;}

	[Doc("""
#Sum[宣告本成員的型別。]

#Descr[
反射源轉發官方 `MemberInfo.DeclaringType`；
Json 源轉發官方 `JsonPropertyInfo.DeclaringType`。

繼承成員的 `DeclaringType` 是基類，與實例的執行期型別不同。
]
""")]
	Type? DeclaringType{get;}

	[Doc("""
#Sum[本成員可讀（可作為字典值來源、可被 `TryGet` 讀取）。]

#Descr[
判據是官方 `Get` 是否為 null（官方 `JsonPropertyInfo` 就是這樣表示可讀）；
反射側同樣以「有沒有公開訪問器」得出 `Get`，兩套來源口徑一致。
]
""")]
	bool CanRead{get;}

	[Doc("""
#Sum[本成員可寫（可被 `TrySet` 寫入）。]

#Descr[
判據同 `CanRead`，取官方 `Set` 是否為 null。
]
""")]
	bool CanWrite{get;}

	[Doc("""
#Sum[官方讀值委託，型別與名字照官方 `JsonPropertyInfo.Get`。]

#Descr[
反射側由 `PropertyInfo.GetValue`／`FieldInfo.GetValue` 建成同一形狀。

不可讀為 null；委託只做取值，前置檢查由 `TryGet` 承擔。
]
""")]
	Func<obj, obj?>? Get{get;}

	[Doc("""
#Sum[官方寫值委託，型別與名字照官方 `JsonPropertyInfo.Set`。]

#Descr[
不可寫為 null。
]
""")]
	Action<obj, obj?>? Set{get;}

	[Doc("""
#Sum[官方特性提供者（兩側共有的那個官方接口）。]

#Descr[
反射源即成員自身；
Json 源取官方 `JsonPropertyInfo.AttributeProvider`。

取值用 `IMemberInfoExtn.GetCustomAttribute<T>()`（官方風格的便利方法），
或直接用官方 `ICustomAttributeProvider.GetCustomAttributes`。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;}

	[Doc("""
#Sum[讀取實例上的本成員。]

#Params([[實例；null 或型別不符時返回 false], [讀出的值；失敗時為 default]])

#Rtn[實例型別不符或本成員不可讀時返回 false]
""")]
	bool TryGet(obj? O, out obj? R);

	[Doc("""
#Sum[寫入實例上的本成員。]

#Params([[實例；null 或型別不符時返回 false], [要寫入的值]])

#Rtn[實例型別不符或本成員不可寫時返回 false]
""")]
	bool TrySet(obj? O, obj? V);
}