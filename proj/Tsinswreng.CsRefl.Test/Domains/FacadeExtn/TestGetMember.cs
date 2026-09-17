using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// GetMember 擴展：命中、未知成員、未註冊型別三種結局。
public partial class TestFacadeExtn{
	public void RegisterGetMember(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestFacadeExtn), [typeof(PoUser)], [nameof(PoUser.Age)], "GetMember:"
		);
		var R = reg.Register;

		R("合成來源 取成員命中", async _ => {
			var T = Assert.IsTrue;
			var M = _merged.GetMember(typeof(PoUser), "Age");
			T(M.CodeName == "Age" && M.DeclaredType == typeof(i32), "應取到 Age 且型別正確");
			return null;
		});

		R("未知成員拋異常並含可用鍵", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_merged.GetMember(typeof(PoUser), "NoSuch");
			}
			catch(KeyNotFoundException E){
				Threw = true;
				T(E.Message.Contains("Age"), $"異常訊息應含可用鍵清單，實際：{E.Message}");
			}
			T(Threw, "未知成員應拋 KeyNotFoundException");
			return null;
		});

		R("未註冊型別 在 Json來源上拋異常", async _ => {
			var T = Assert.IsTrue;
			// 直接用只認已註冊型別的 Json 來源測「未註冊」分叉。
			var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
			var Threw = false;
			try{
				JsonOnly.GetMember(typeof(PoNoCtor), "X");
			}
			catch(KeyNotFoundException E){
				Threw = true;
				T(E.Message.Contains("未註冊"), $"型別未註冊的訊息應明說，實際：{E.Message}");
			}
			T(Threw, "未註冊型別應拋 KeyNotFoundException");
			return null;
		});
	}
}