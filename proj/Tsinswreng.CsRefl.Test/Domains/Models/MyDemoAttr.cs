namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 測試用的特性：標在被測模型的屬性上（如 Level）。
/// 用來驗證「兩套來源都能透過成員的 AttributeProvider 查到特性」的契約
/// （實測：反射源的提供者是成員自身；源生成下 JsonPropertyInfo.AttributeProvider 也取得到本特性，
/// 故 AOT 下特性查詢不必另走反射）。
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MyDemoAttr:Attribute{
	/// 標籤文本。
	public str Tag{get;}
	/// 等級數值。
	public i32 Rank{get;}

	public MyDemoAttr(str Tag, i32 Rank){
		this.Tag = Tag;
		this.Rank = Rank;
	}
}
