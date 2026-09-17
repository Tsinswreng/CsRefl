using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// 視圖形狀：鍵序 = 可讀可寫成員的宣告序（修「舊 PropDict 鍵排序變字母序」），
/// Values 與鍵一一對應且不排序（修「異質值排序拋錯」），IsReadOnly=false。
public partial class TestInstDict{
	public void RegisterShape(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestInstDict), [typeof(PoUser)], [nameof(PoUser.Age)], "視圖形狀:"
		);
		var R = reg.Register;

		R("鍵序等於成員序", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			T(Dict.Keys.SequenceEqual([
				"Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Level", "Note",
			]), $"鍵序應是宣告序且排除只讀 Secret，實際 {string.Join(",", Dict.Keys)}");
			T(Dict.Count == 9, "鍵數應是 9");
			return null;
		});

		R("Values 與鍵一一對應", async _ => {
			var T = Assert.IsTrue;
			var User = MakeUser();
			var Dict = MakeDict(User);
			var Values = Dict.Values.ToList();
			T((i64)Values[0]! == 1, "Values[0] 應是 Id=1");
			T((str)Values[1]! == "小明", "Values[1] 應是 Name");
			T((i32)Values[7]! == 3, "Values[7] 應是 Level=3");
			// 異質值（i64/str/i32/bool/字典）混排不拋——舊 PropDict 的拋錯點。
			T(Values.Count == 9, "Values 應有 9 項");
			return null;
		});

		R("IsReadOnly 為 false", async _ => {
			var T = Assert.IsTrue;
			T(!MakeDict(MakeUser()).IsReadOnly, "視圖要能被當普通字典改值（JsonNode 下游兼容），IsReadOnly 應為 false");
			return null;
		});

		R("Keys 集合常駐且與 Count 一致", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			T(Dict.Keys.Contains("Level") && !Dict.Keys.Contains("Secret"), "Keys 集合應含 Level 不含 Secret");
			T(Dict.Keys.Count == Dict.Count, "Keys.Count 應等於 Count");
			return null;
		});
	}
}