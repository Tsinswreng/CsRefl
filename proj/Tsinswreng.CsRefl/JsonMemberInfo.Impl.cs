namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonMemberInfo)} 的函數實現。]

#Descr[
只放函數實現：成員事實字段與訪問器在 `JsonMemberInfo.cs`。
]
""")]
public partial class JsonMemberInfo{
	[Doc($"""
#Sum[包一個 {nameof(JsonPropertyInfo)}。]

#Descr[
例：`new {nameof(JsonMemberInfo)}(Info.{nameof(JsonTypeInfo.Properties)}[0])` 之後
{nameof(Json)} 是那個官方成員、{nameof(Member)} 為 null、
{nameof(Get)} 與 {nameof(Set)} 就是官方源生成的委託。
]

#See[{nameof(JsonMemberInfo)}]
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Prop)
		: base(
			// 反射成員出口為 null：JsonPropertyInfo 不是 MemberInfo 的子類
			// （官方這兩個體系沒有共同基類），故只交給 Json 出口。
			Member: null,
			Json: Prop,
			// 名字語義：官方 JsonPropertyInfo.Name 是 JSON 名；門面用它當鍵。
			// 與 C# 名相等的前提由調用方保證（命名策略為 null 且無 [JsonPropertyName]）。
			Name: Prop.Name,
			PropertyType: Prop.PropertyType,
			DeclaringType: Prop.DeclaringType,
			// 官方 JsonPropertyInfo 不暴露 IsProperty，分不出源生成收進來的字段
			// （[JsonInclude] 的字段在實測中連 AttrProvider 都給 RtFieldInfo），
			// 故這裡統一報 Property——這是兩套來源記錄在案的能力差別。
			MemberType: System.Reflection.MemberTypes.Property,
			// 讀寫委託直接取官方 JsonPropertyInfo.Get/Set，
			// 是 AOT 下最優路徑（源生成的委託，零反射），型別與門面宣告完全一致。
			// 可讀/可寫與委託同一判據（官方就是用 Get/Set 是否為 null 表示可讀可寫）。
			CanRead: Prop.Get is not null,
			CanWrite: Prop.Set is not null,
			Get: Prop.Get,
			Set: Prop.Set,
			// 官方提供的特性提供者；源生成下實測可取到特性（見 JsonMemberInfo 的說明）。
			AttributeProvider: Prop.AttributeProvider
		)
	{
	}
}