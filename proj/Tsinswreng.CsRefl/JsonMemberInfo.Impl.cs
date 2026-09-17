namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonMemberInfo 的函數實現。
public partial class JsonMemberInfo{
	/// 包一個 JsonPropertyInfo。讀寫委託直接取 JsonPropertyInfo.Get/Set，
	/// 是 AOT 下最優路徑（源生成的委託，零反射）。
	public JsonMemberInfo(JsonPropertyInfo Prop)
		: base(Prop.Get, Prop.Set)
	{
		// 名字語義：JsonPropertyInfo.Name 是 JSON 名；CodeName 與 JsonName 同源。
		// 與 C# 名相等的前提由調用方保證（命名策略為 null 且無 [JsonPropertyName]）。
		var Name = Prop.Name;
		CodeName = Name;
		JsonName = Name;
		DeclaredType = Prop.PropertyType;
		DeclaringType = Prop.DeclaringType!;
		CanRead = Prop.Get is not null;
		CanWrite = Prop.Set is not null;
		Order = Prop.Order;
	}
}