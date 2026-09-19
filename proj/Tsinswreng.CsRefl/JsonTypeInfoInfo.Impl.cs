namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonTypeInfoInfo)} 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `JsonTypeInfoInfo.cs`。
]
""")]
public partial class JsonTypeInfoInfo{
	[Doc($"""
#Sum[包一個 {nameof(JsonTypeInfo)}。]

#Descr[
例：成員表由官方 {nameof(JsonTypeInfo.Properties)} 一次轉出，
{nameof(JsonTypeInfo.Kind)}、{nameof(JsonTypeInfo.ElementType)}、{nameof(JsonTypeInfo.KeyType)}
全部照官方轉發，故本包裝層不重複實現官方的分類邏輯。
]

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

	[Doc($"""
#Sum[建立實例，轉調官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 工廠。]

#Descr[
例：掛了 `[JsonSerializable]` 且可建實例的型別，這裡返回一個新實例；
官方沒給工廠時拋 {nameof(NotSupportedException)}，訊息說明是官方 `CreateObject` 為 null。
]

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

	[Doc($"""
#Sum[把官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.Properties)} 包成成員表。]

#Params([[官方型別元資料]])

#Rtn[包好的成員表（尚未規整，由建構子統一處理）]

#Descr[
注意：官方 {nameof(JsonPropertyInfo.Name)} 是 JSON 名，
與 C# 名相等的前提由調用方保證（命名策略為 null 且無 `[JsonPropertyName]`）。

例：官方有 11 個成員時這裡就轉出 11 個 {nameof(JsonMemberInfo)}，
順序保持官方既有序，之後由 {nameof(TypeInfoSorter)} 規整。
]
""")]
	private static IReadOnlyList<IMemberInfo> CollectMembers(JsonTypeInfo Json){
		// 先按官方成員數定容量，省掉過程中的擴容。
		var R = new List<IMemberInfo>(Json.Properties.Count);
		foreach(var P in Json.Properties){
			R.Add(new JsonMemberInfo(P));
		}
		return R;
	}
}