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
實測（`PoUser`）：成員表由官方 {nameof(JsonTypeInfo.Properties)} 一次轉出（官方 11 項）；
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
		_Json = Json;
	}

	[Doc($"""
#Sum[建立實例，轉調官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 工廠。]

#Descr[
實測：`PoUser`（掛了 `[JsonSerializable]`、有公開無參構造函數）
這裡返回一個 `PoUser` 新實例，其 `Id` 可立即賦值；
官方沒給工廠時（如 `PoNoCtor`）拋 {nameof(NotSupportedException)}，
訊息說明是官方 `CreateObject` 為 null。
]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst(){
		// 能走到這裏說明 CreateObject 非 null（CanMkInst 為 true），null 分支只是防禦。
		var F = _Json.CreateObject
			?? throw new NotSupportedException(
				$"型別 {Type.FullName} 在 JsonTypeInfo 中沒有無參工廠（CreateObject 為 null），無法建立實例。"
			);
		return F();
	}

	private static partial IReadOnlyList<IMemberInfo> CollectMembers(JsonTypeInfo Json){
		// 先按官方成員數定容量，省掉過程中的擴容。
		var R = new List<IMemberInfo>(Json.Properties.Count);
		foreach(var P in Json.Properties){
			R.Add(new JsonMemberInfo(P));
		}
		return R;
	}
}
