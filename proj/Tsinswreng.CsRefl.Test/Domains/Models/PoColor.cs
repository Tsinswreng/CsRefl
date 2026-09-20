namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 枚舉樣例：驗證反射來源把它分類為標量（Json 來源不註冊枚舉時查不到，
/// 這裡註冊是為了讓兩套來源都能覆蓋枚舉的 Kind 契約）。
public enum PoColor{
	/// 紅。
	Red = 0,
	/// 綠。
	Green = 1,
	/// 藍。
	Blue = 2,
}
