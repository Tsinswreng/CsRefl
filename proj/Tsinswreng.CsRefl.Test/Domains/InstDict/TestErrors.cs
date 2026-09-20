using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// 視圖異常路徑：形狀固定（增刪清空不允許）、未知鍵、寫只讀成員。
/// 函數實現文件；聲明在 _TestInstDict.cs。
public partial class TestInstDict{
	/// 見聲明處的說明。
	public partial void RegisterErrors(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestInstDict), [typeof(PoUser)], [nameof(PoUser.Age)], "視圖異常:"
		);
		var R = reg.Register;

		R("Add/Remove/Clear 一律不允許", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			var Threw = 0;
			try{
				Dict.Add("NewKey", 1);
			}
			catch(NotSupportedException){
				Threw++;
			}
			try{
				Dict.Remove("Age");
			}
			catch(NotSupportedException){
				Threw++;
			}
			try{
				Dict.Clear();
			}
			catch(NotSupportedException){
				Threw++;
			}
			T(Threw == 3, "Add/Remove/Clear 都應拋 NotSupportedException");
			return null;
		});

		R("未知鍵讀寫拋異常且含可用鍵", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			var ReadThrew = false;
			try{
				_ = Dict["NoSuch"];
			}
			catch(KeyNotFoundException E){
				ReadThrew = true;
				T(E.Message.Contains("Level"), $"讀異常訊息應含可用鍵，實際：{E.Message}");
			}
			var WriteThrew = false;
			try{
				Dict["NoSuch"] = 1;
			}
			catch(KeyNotFoundException E){
				WriteThrew = true;
				T(E.Message.Contains("Level"), $"寫異常訊息應含可用鍵，實際：{E.Message}");
			}
			T(ReadThrew && WriteThrew, "未知鍵讀/寫都應拋 KeyNotFoundException");
			return null;
		});

		R("寫只讀成員拋異常", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			// Secret 是成員但可讀不可寫：寫它應拋 InvalidOperationException。
			var Threw = false;
			try{
				Dict["Secret"] = "x";
			}
			catch(InvalidOperationException E){
				Threw = true;
				T(E.Message.Contains("Secret"), $"異常訊息應指名成員，實際：{E.Message}");
			}
			T(Threw, "寫只讀成員應拋 InvalidOperationException");
			return null;
		});

		R("鍵表外的可讀成員允許讀", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			// 設計記録：索引器讀只查成員表不查鍵表，只讀成員也能按名讀。
			T((str)Dict["Secret"]! == "s", "Dict[Secret] 應能讀到 s");
			return null;
		});

		R("只寫成員寫得進但不在鍵表", async _ => {
			var T = Assert.IsTrue;
			var User = MakeUser();
			var Dict = MakeDict(User);
			// Token 可寫不可讀：寫回物件有效，但不是視圖的鍵（讀不到、ContainsKey false）。
			Dict["Token"] = "t1";
			T(User.TokenEcho == "t1", "寫只寫成員應寫回物件");
			T(!Dict.ContainsKey("Token"), "只寫成員不應出現在鍵表");
			T(!Dict.TryGetValue("Token", out _), "只寫成員取不到值");
			var Threw = false;
			try{
				_ = Dict["Token"];
			}
			catch(KeyNotFoundException){
				Threw = true;
			}
			T(Threw, "讀只寫成員應拋 KeyNotFoundException");
			return null;
		});
	}
}
