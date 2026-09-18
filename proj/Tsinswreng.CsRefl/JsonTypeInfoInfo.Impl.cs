namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfoInfo 的函數實現。
/// 只放函數實現：型別事實字段與訪問器在 JsonTypeInfoInfo.cs。
public partial class JsonTypeInfoInfo{
	/// 包一個 JsonTypeInfo。成員直接沿用 Json.Properties 的既有序，由 TypeInfoBase
	/// 建構子統一規整成契約序並去重。Kind／ElementType／KeyType 全部照官方轉發。
	public partial JsonTypeInfoInfo(JsonTypeInfo Json)
		: base(
			Type: Json.Type,
			Kind: Json.Kind,
			Members: CollectMembers(Json),
			ElementType: Json.ElementType,
			KeyType: Json.KeyType
		)
	{
		_json = Json;
	}

	/// 建立實例，轉調官方 CreateObject 工廠。
	/// 能走到這裏說明 CreateObject 非 null（CanMkInst 為 true），null 分支只是防禦。
	public override partial obj? MkInst(){
		var F = _json.CreateObject
			?? throw new NotSupportedException(
				$"型別 {Type.FullName} 在 JsonTypeInfo 中沒有無參工廠（CreateObject 為 null），無法建立實例。"
			);
		return F();
	}

	/// 把官方 Json.Properties 包成成員表。
	/// 注意：JsonPropertyInfo.Name 是 JSON 名，與 C# 名相等的前提由調用方保證
	/// （命名策略為 null 且無 [JsonPropertyName]）。
	private static IReadOnlyList<IMemberInfo> CollectMembers(JsonTypeInfo Json){
		var R = new List<IMemberInfo>(Json.Properties.Count);
		foreach(var P in Json.Properties){
			R.Add(new JsonMemberInfo(P));
		}
		return R;
	}
}