namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 遮蔽測試的派生類：用 new 遮蔽基類的 Id。
///
/// 為甚麼要有這個模型：成員名就是字典鍵 / SQL 列名，同名成員若有兩份就會讓
/// Members、ReadableNames、按名索引三處口徑分裂。這裡驗證三段結果：
/// - 成員總數 3，且 Id 只出現一次；
/// - 成員序 Name、Id、Age（基類成員在前、同類內宣告序）；
/// - 按名查到的 Id 是派生類那份宣告（OwnerType == PoUserExt）。
///
/// 實測事實（.NET 10，見 TestShadowing 的斷言）：Type.GetProperties 本身就不返回
/// 被 new 遮蔽的基類屬性，兩套來源都不產生重複名。也就是說門面的按名去重是
/// 防禦性的，不是修某個必然出現的 bug。
public class PoUserExt:PoUserExtBase{
	/// 遮蔽基類的 Id：宣告型別是 PoUserExt，不是 PoUserExtBase。
	public new i64 Id{get;set;}
	/// 派生類獨有的成員。
	public i32 Age{get;set;}
}
