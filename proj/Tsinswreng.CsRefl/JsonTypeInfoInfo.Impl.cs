namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`JsonTypeInfoInfo` 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `JsonTypeInfoInfo.cs`。
]
""")]
public partial class JsonTypeInfoInfo{
	[Doc("""
#Sum[包一個 `JsonTypeInfo`。]

#See[{nameof(JsonTypeInfoInfo)}]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json)
		: base(
			// 成員直接沿用 Json.Properties 的既有序，由 TypeInfoBase 建構子
			// 統一規整成契約序並去重。
			// Kind／ElementType／KeyType 全部照官方轉發。
			Type: Json.Type,
			Kind: Json.Kind,
			Members: CollectMembers(Json),
			ElementType: Json.ElementType,
			KeyType: Json.KeyType
		)
	{
		_json = Json;
	}

	[Doc("""
#Sum[建立實例，轉調官方 `CreateObject` 工廠。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst(){
		// 能走到這裏說明 CreateObject 非 null（CanMkInst 為 true），null 分支只是防禦。
		var F = _json.CreateObject
			?? throw new NotSupportedException(
				$"型別 {Type.FullName} 在 JsonTypeInfo 中沒有無參工廠（CreateObject 為 null），無法建立實例。"
			);
		return F();
	}

	[Doc("""
#Sum[把官方 `Json.Properties` 包成成員表。]

#Params([[官方型別元資料]])

#Rtn[包好的成員表（尚未規整，由建構子統一處理）]

#Descr[
注意：`JsonPropertyInfo.Name` 是 JSON 名，
與 C# 名相等的前提由調用方保證（命名策略為 null 且無 `[JsonPropertyName]`）。
]
""")]
	private static IReadOnlyList<IMemberInfo> CollectMembers(JsonTypeInfo Json){
		var R = new List<IMemberInfo>(Json.Properties.Count);
		foreach(var P in Json.Properties){
			R.Add(new JsonMemberInfo(P));
		}
		return R;
	}
}