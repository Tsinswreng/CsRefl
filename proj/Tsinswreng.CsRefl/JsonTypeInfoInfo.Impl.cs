namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfoInfo 的函數實現。
public partial class JsonTypeInfoInfo{
	/// 包一個 JsonTypeInfo。
	public JsonTypeInfoInfo(JsonTypeInfo Json){
		_json = Json;
		_type = Json.Type;
		_kind = Json.Kind switch{
			JsonTypeInfoKind.Object => ETypeKind.Object,
			JsonTypeInfoKind.Enumerable => ETypeKind.Enumerable,
			JsonTypeInfoKind.Dictionary => ETypeKind.Dictionary,
			_ => ETypeKind.Scalar,
		};
		var Members = new List<IMemberInfo>(Json.Properties.Count);
		foreach(var P in Json.Properties){
			// JsonTypeInfo.Properties 已完成 Order+宣告序排序，本類直接沿用；
			// 統一排序器基類在前（STJ 默認是派生類成員在前）把它整成契約序。
			Members.Add(new JsonMemberInfo(P));
		}
		_members = TypeInfoSorter.Sort(Json.Type, Members);
		_elemType = Json.ElementType;
		_keyType = Json.KeyType;
	}

	/// 建立實例，轉調 CreateObject 工廠；無無參工廠時拋 NotSupportedException。
	public override obj? MkInst(){
		var F = _json.CreateObject
			?? throw new NotSupportedException(
				$"型別 {_type.FullName} 在 JsonTypeInfo 中沒有無參工廠（CreateObject 為 null），無法建立實例。"
			);
		return F();
	}
}