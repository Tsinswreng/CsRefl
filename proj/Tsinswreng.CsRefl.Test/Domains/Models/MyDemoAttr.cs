namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 測試用的特性：標在被測模型的屬性上（如 Level），
/// 用來驗證「反射來源能查到特性、JsonTypeInfo 來源查不到」的契約差別。
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