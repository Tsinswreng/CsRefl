namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 測試模型的基類：驗證「繼承成員也進成員表、且排在派生類成員前面」。
/// 兩個可讀可寫屬性，供 ToInstDict/AssignFromDict 用例覆蓋繼承鏈。
public class PoUserBase{
	/// 主鍵。
	public i64 Id{get;set;}
	/// 名字。
	public str Name{get;set;} = "";
}
