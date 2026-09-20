namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 遮蔽測試的基類：提供 Id / Name 兩個公開可讀寫屬性。
/// 派生類 PoUserExt 用 new 遮蔽 Id，用於驗證成員表的同名去重與成員序。
public class PoUserExtBase{
	/// 主鍵（將被派生類遮蔽）。
	public i64 Id{get;set;}
	/// 名字（不被遮蔽，用於驗證「非遮蔽成員序不變」）。
	public str Name{get;set;} = "";
}
