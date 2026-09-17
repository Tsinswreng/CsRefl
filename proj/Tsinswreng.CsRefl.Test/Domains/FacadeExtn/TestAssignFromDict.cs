using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// AssignFromDict 擴展：字典寫回物件的各種路徑。
public partial class TestFacadeExtn{
	public void RegisterAssignFromDict(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestFacadeExtn), [typeof(PoUser)], [nameof(PoUser.Age)], "AssignFromDict:"
		);
		var R = reg.Register;

		R("寫回可寫鍵", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser();
			var Dict = new Dictionary<str, obj?>{
				["Id"] = 9L,
				["Name"] = "小紅",
				["Level"] = 7,
			};
			_merged.AssignFromDict(User, Dict);
			T(User.Id == 9 && User.Name == "小紅" && User.Level == 7, "三個鍵應全部寫回");
			return null;
		});

		R("只讀鍵跳過不拋", async _ => {
			var T = Assert.IsTrue;
			var User = new PoUser();
			var Dict = new Dictionary<str, obj?>{
				["Level"] = 4,
				["Secret"] = "改不掉",
			};
			_merged.AssignFromDict(User, Dict);
			T(User.Level == 4, "可寫鍵應寫回");
			T(User.Secret == "s", "只讀鍵應被跳過（不拋錯也不改值）");
			return null;
		});

		R("未知鍵拋異常並含可用可寫鍵", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_merged.AssignFromDict(new PoUser(), new Dictionary<str, obj?>{ ["NoSuch"] = 1 });
			}
			catch(KeyNotFoundException E){
				Threw = true;
				T(E.Message.Contains("Age"), $"異常訊息應含可用可寫鍵清單，實際：{E.Message}");
			}
			T(Threw, "未知鍵應拋 KeyNotFoundException");
			return null;
		});

		R("值型別不符拋異常", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_merged.AssignFromDict(new PoUser(), new Dictionary<str, obj?>{ ["Age"] = "不是數字" });
			}
			catch(InvalidOperationException){
				Threw = true;
			}
			T(Threw, "值型別不符應拋 InvalidOperationException");
			return null;
		});
	}
}