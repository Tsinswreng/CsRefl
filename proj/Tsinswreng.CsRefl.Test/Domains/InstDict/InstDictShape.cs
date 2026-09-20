using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// 視圖形狀一份，供各文件共用：
/// - 出現口徑的鍵 = PoUser 上「可讀且可寫」的成員名，順序同成員序；
/// - 出現口徑的鍵數。
/// 動了 PoUser 的成員就只改這裡，不必在每條用例裏重抄一遍清單。
internal static class InstDictShape{
	/// 期望鍵序（見 PoUser 的成員注釋）：排除只讀 Secret、排除只寫 Token。
	public static readonly str[] ExpectedKeys = [
		"Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Level", "Note",
	];

	/// 期望鍵數。
	public static i32 ExpectedCount{
		get{
			return ExpectedKeys.Length;
		}
	}
}
