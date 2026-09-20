using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// 視圖讀寫：索引器、ContainsKey、TryGetValue 都直接作用在背後的物件上。
/// 函數實現文件；聲明在 _TestInstDict.cs。
public partial class TestInstDict{
	/// 見聲明處的說明。
	public partial void RegisterReadWrite(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestInstDict), [typeof(PoUser)], [nameof(PoUser.Age)], "視圖讀寫:"
		);
		var R = reg.Register;

		R("索引器讀取", async _ => {
			var T = Assert.IsTrue;
			var User = MakeUser();
			var Dict = MakeDict(User);
			T((i32)Dict["Age"]! == 26, "Dict[Age] 應讀到 26");
			T((str)Dict["Name"]! == "小明", "Dict[Name] 應讀到 小明");
			return null;
		});

		R("索引器寫回物件", async _ => {
			var T = Assert.IsTrue;
			var User = MakeUser();
			var Dict = MakeDict(User);
			Dict["Level"] = 8;
			T(User.Level == 8, "Dict[Level]=8 應寫回物件");
			Dict["Name"] = "阿強";
			T(User.Name == "阿強", "Dict[Name]=阿強 應寫回物件");
			return null;
		});

		R("ContainsKey 與 TryGetValue", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			T(Dict.ContainsKey("Age"), "ContainsKey 應命中 Age");
			T(!Dict.ContainsKey("NoSuch"), "ContainsKey 對未知鍵應 false");
			T(Dict.TryGetValue("Age", out var V) && (i32)V! == 26, "TryGetValue 應命中");
			T(!Dict.TryGetValue("NoSuch", out _), "TryGetValue 對未知鍵應 false");
			return null;
		});

		R("枚舉視圖", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			var Pairs = Dict.ToList();
			T(Pairs.Count == InstDictShape.ExpectedCount, $"枚舉應有 {InstDictShape.ExpectedCount} 對");
			T(Pairs[0].Key == "Id" && (i64)Pairs[0].Value! == 1, "第一對應是 Id=1");
			T(Pairs[6].Key == "Extra", "第七對應是 Extra");
			return null;
		});

		R("CopyTo 按鍵序拷貝", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			// 留兩個空位，順帶驗證「自 ArrayIndex 起」寫入。
			var Array = new KeyValuePair<str, obj?>[InstDictShape.ExpectedCount + 2];
			// 下標 0/1 放哨兵，驗證 CopyTo 不會動它們。
			Array[0] = new KeyValuePair<str, obj?>("Before0", null);
			Array[1] = new KeyValuePair<str, obj?>("Before1", null);
			Dict.CopyTo(Array, 2);
			T(Array[0].Key == "Before0" && Array[1].Key == "Before1", "下標 0/1 應保持原樣");
			T(Array[2].Key == "Id" && (i64)Array[2].Value! == 1, "拷貝應自 ArrayIndex 起按鍵序寫入");
			T(Array[InstDictShape.ExpectedCount + 1].Key == "Note", "最後一項應是 Note");
			return null;
		});

		R("CopyTo 容量不足拋 ArgumentException", async _ => {
			var T = Assert.IsTrue;
			var Dict = MakeDict(MakeUser());
			var Threw = false;
			try{
				Dict.CopyTo(new KeyValuePair<str, obj?>[3], 0);
			}
			catch(ArgumentException){
				Threw = true;
			}
			T(Threw, "容量不足應拋 ArgumentException（修前裸拋 IndexOutOfRange）");
			return null;
		});
	}
}
