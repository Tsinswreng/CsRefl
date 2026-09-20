using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// TryGet/TrySet 擴展：按名讀寫的命中與各種 false 分叉。
/// 函數實現文件；聲明在 _TestFacadeExtn.cs。
public partial class TestFacadeExtn{
	/// 見聲明處的說明。
	public partial void RegisterTryGetTrySet(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestFacadeExtn), [typeof(PoUser)], [nameof(PoUser.Age)], "TryGet/TrySet:"
		);
		var R = reg.Register;

		R("TryGet 讀值", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser{ Age = 26 };
			T(_merged.TryGet(typeof(PoUser), User, "Age", out var R2), "TryGet 應命中");
			T((i32)R2! == 26, $"Age 讀值應為 26，實際 {R2}");
			return null;
		});

		R("TryGet 可空未賦值讀到 null", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser();
			T(_merged.TryGet(typeof(PoUser), User, "Email", out var R2), "TryGet Email 應命中");
			T(R2 is null, "Email 未賦值應為 null");
			return null;
		});

		R("TrySet 寫回物件", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser{ Married = false };
			T(_merged.TrySet(typeof(PoUser), User, "Married", true), "TrySet 應命中");
			T(User.Married, "Married 應寫回 true");
			return null;
		});

		R("Try拿 各種失敗分叉", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser();
			T(!_merged.TryGet(typeof(PoUser), User, "NoSuch", out _), "未知成員應 false");
			T(!_merged.TryGet(typeof(PoUser), null, "Age", out _), "null 實例應 false");
			T(!_merged.TryGet(typeof(PoColor), User, "Age", out _), "型別與實例不符應 false");
			T(!_merged.TrySet(typeof(PoUser), User, "NoSuch", 1), "TrySet 未知成員應 false");
			T(!_merged.TrySet(typeof(PoUser), User, "Secret", "x"), "TrySet 只讀成員應 false");
			return null;
		});
	}
}