using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 用法三：把物件當字典用（表單綁定、可視化、整體覆蓋既有成員）。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> ObjectAsDict(obj? O){
		var T = Assert.IsTrue;

		// 起點：門面。
		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);

		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		// 視圖是物件的「淺字典外殼」：不複製成員值，讀寫都落在原物件上。
		var Dict = Src.ToInstDict(U);
		T(ReferenceEquals(Dict.Target, U), "視圖背後就是那個物件");
		T(Dict.TypeInfo.Type == typeof(PoUser), "視圖帶着建它用的那份元資料");

		// 鍵表 = 可讀可寫成員，順序 = 成員序，可直接當表格的欄位序。
		T(Dict.Count == 9, $"可讀可寫鍵應有 9 個，實際 {Dict.Count}");
		T(Dict.Keys.SequenceEqual([
			nameof(PoUser.Id), nameof(PoUser.Name), nameof(PoUser.Age), nameof(PoUser.Email),
			nameof(PoUser.Married), nameof(PoUser.Tags), nameof(PoUser.Extra),
			nameof(PoUser.Level), nameof(PoUser.Note),
		]), "鍵序應是成員序（只讀的 Secret 與只寫的 Token 都不是鍵）");

		// 索引器寫的是原物件：表單改了哪一格，物件就跟着變。
		Dict[nameof(PoUser.Age)] = 32;
		T(U.Age == 32, "寫視圖應寫回原物件");
		T((i32)Dict[nameof(PoUser.Age)]! == 32, "讀視圖應讀到原物件的值");

		// 出現口徑（鍵表）與訪問口徑（索引器）是兩回事：
		// 只讀成員讀得到、但不是鍵；只寫成員寫得進、但讀不到。
		T((str)Dict[nameof(PoUser.Secret)]! == "s", "只讀成員讀得到值");
		T(!Dict.ContainsKey(nameof(PoUser.Secret)), "只讀成員不是鍵（整體覆蓋時蓋不到它）");
		Dict[nameof(PoUser.Token)] = "t1";
		T(U.TokenEcho == "t1", "只寫成員寫得進物件");
		T(!Dict.TryGetValue(nameof(PoUser.Token), out _), "只寫成員取不到值");

		// 形狀由型別成員固定：拿到 IDictionary 也不能增刪鍵，只能改既有成員的值。
		var Threw = false;
		try{
			Dict.Add("NewKey", 1);
		}catch(NotSupportedException){
			Threw = true;
		}
		T(Threw, "Add 應拋 NotSupportedException");

		// 只要基類那一部分時顯式給型別：子類新增的成員就不在鍵表裏。
		var BaseDict = Src.ToInstDict(U, typeof(PoUserBase));
		T(BaseDict.Count == 2, $"基類視圖應只有 2 個鍵，實際 {BaseDict.Count}");
		T(BaseDict.Keys.SequenceEqual([nameof(PoUserBase.Id), nameof(PoUserBase.Name)]), "基類視圖的鍵應是 Id、Name");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterDictView(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(IInstDict), typeof(ITypeInfoSrcExtn)],
			[nameof(ITypeInfoSrcExtn.ToInstDict), nameof(IInstDict.Keys)],
			"綜合測試:物件當字典:"
		);
		reg.Register(nameof(ObjectAsDict), ObjectAsDict!);
	}
}
