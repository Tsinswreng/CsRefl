namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonTypeInfoInfo)} 的函數實現。]

#Descr[
只放函數實現：屬性與字段在 `JsonTypeInfoInfo.cs`。
]
""")]
public partial class JsonTypeInfoInfo{
	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
基礎事實全部取自官方那一個實例，成員表取官方 {nameof(JsonTypeInfo.Properties)}，
每一項包成 {nameof(JsonMemberInfo)}；排序去重統一交給 {nameof(TypeInfoBase)}。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json)
		: base(
			// 參數求值自左向右，故 null 檢查放在第一個實參上。
			Type: (Json ?? throw new ArgumentNullException(nameof(Json))).Type,
			Kind: Json.Kind,
			// 官方 Properties 是 IList<JsonPropertyInfo>，逐項包成 IMemberInfo 收成成員表。
			Members: Json.Properties.Select(P => (IMemberInfo)new JsonMemberInfo(P)).ToList(),
			ElementType: Json.ElementType,
			KeyType: Json.KeyType
		)
	{
		// 官方本體直接落在屬性上（不再另存欄位轉發）；參數同名，故用 this.。
		this.Json = Json;
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
實測：`typeof(PoUser)` 建得出實例；沒有官方工廠的型別拋 {nameof(NotSupportedException)}，訊息含型別全名。
]
""")]
	public override partial obj? MkInst(){
		// 錯誤訊息與 CanMkInst 用同一個判據：官方 CreateObject 是否為 null。
		var J = Json!;
		var F = J.CreateObject;
		if(F is null){
			throw new NotSupportedException(
				$"型別 {J.Type.FullName} 的官方元資料沒有 CreateObject，無法建立實例。"
			);
		}
		return F();
	}
}


