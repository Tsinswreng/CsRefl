using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// ToInstDict 擴展：預設執行期型別、顯式型別、null 實例、邊界型別。
public partial class TestFacadeExtn{
	public void RegisterToInstDict(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestFacadeExtn), [typeof(PoUser)], [nameof(PoUser.Age)], "ToInstDict:"
		);
		var R = reg.Register;

		R("執行期型別建視圖", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser{ Id = 1 };
			var Dict = _merged.ToInstDict(User);
			T(Dict.Target == User, "視圖目標應是原物件");
			T(Dict.Count == 9, $"可讀可寫鍵應有 9 個（Secret 只讀被排除），實際 {Dict.Count}");
			T((i64)Dict["Id"]! == 1, "視圖讀 Id 應為 1");
			return null;
		});

		R("顯式基類型別建視圖", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser{ Id = 5, Name = "小明" };
			// 傳 typeof(PoUserBase)：視圖只認基類成員（修「typeof(T) 查不到基類成員」隱患的對照用例）。
			var Dict = _merged.ToInstDict(User, typeof(PoUserBase));
			T(Dict.Count == 2, $"基類視圖應只有 Id/Name 兩個鍵，實際 {Dict.Count}");
			T(Dict.Keys.SequenceEqual([ "Id", "Name" ]), "基類視圖鍵應是 Id、Name");
			return null;
		});

		R("null 實例拋異常", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_merged.ToInstDict(null!);
			}
			catch(ArgumentNullException){
				Threw = true;
			}
			T(Threw, "ToInstDict(null) 應拋 ArgumentNullException");
			return null;
		});

		R("只讀邊界型別 空視圖", async _ => {
			var T = Assert.IsTrue;
			var Dict = _merged.ToInstDict(new PoNoCtor(9));
			T(Dict.Count == 0, "PoNoCtor 只有只讀成員 X，視圖應為空");
			return null;
		});
	}
}