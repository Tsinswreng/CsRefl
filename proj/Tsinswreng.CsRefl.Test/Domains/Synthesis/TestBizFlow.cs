using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 端到端：落庫與回填——把門面層、元資料層、視圖層串成一段業務流程。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> BizRowAndFillBack(obj? O){
		var T = Assert.IsTrue;

		// 起點：調用方自己把門面建出來（生產是合成來源，注入進來的也是這一個）。
		ITypeInfoSrc Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);

		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		// 要落庫的列 = 可讀可寫成員名，順序就是成員序，可直接當 SQL 的列序。
		// 這份清單按實例緩存，故在業務代碼裏反復讀不會反復計算。
		var Info = Src.GetInfo<PoUser>();
		var Cols = Info.ReadWriteMembers.Keys.ToList();
		T(Cols.Count == 9, $"可讀可寫成員應有 9 個，實際 {Cols.Count}");
		T(Cols.SequenceEqual([
			nameof(PoUser.Id), nameof(PoUser.Name), nameof(PoUser.Age), nameof(PoUser.Email),
			nameof(PoUser.Married), nameof(PoUser.Tags), nameof(PoUser.Extra),
			nameof(PoUser.Level), nameof(PoUser.Note),
		]), "列序應是成員序（只讀的 Secret 與只寫的 Token 都不是列）");

		// 列名是運行期才知道的，所以這裏按名字逐列取值；
		// TryGet 不拋——名字髒了就返回 false，適合批量回填。
		var Vals = new List<obj?>();
		foreach(var Col in Cols){
			T(Src.TryGet(typeof(PoUser), U, Col, out var V), $"{Col} 是列，應取得到值");
			Vals.Add(V);
		}
		T(Vals.Count == Cols.Count, "列與值應一一對應");
		T((i64)Vals[0]! == 1, "第 1 列 Id 應是 1");
		T((str)Vals[1]! == "小明", "第 2 列 Name 應是小明");
		T((i32)Vals[2]! == 26, "第 3 列 Age 應是 26");

		// 回填：字典的鍵就是成員名；可寫鍵寫入、只讀鍵靜默跳過。
		Src.AssignFromDict(U, new Dictionary<str, obj?>{
			[nameof(PoUser.Age)] = 31,
			[nameof(PoUser.Name)] = "小紅",
			[nameof(PoUser.Secret)] = "改不掉",
		});
		T(U.Age == 31 && U.Name == "小紅", "可寫鍵應寫回物件");
		T(U.Secret == "s", "只讀鍵應被跳過：不拋、也不改值");

		// 字典裏出現型別上沒有的鍵就不是跳過而是拋——
		// 回填多發生在反序列化路徑上，靜默丟鍵比當場拋更難查。
		var Threw = false;
		try{
			//TswgNote 爲甚麼這麼設計? 那我子類的Dict 賦到接口上你不炸了?
			//甚麼時候該靜默 甚麼時候該拋異常分不清楚嗎? 這時候靜默又能怎樣?
			Src.AssignFromDict(U, new Dictionary<str, obj?>{["NoSuch"] = 1});
		}catch(KeyNotFoundException){
			Threw = true;
		}
		T(Threw, "未知鍵應拋 KeyNotFoundException");

		// ================= 泛型版（型別編譯期已知）=================
		// 同一段回填用泛型版寫：只認 T 那張成員表。
		Src.AssignFromDict<PoUser>(U, new Dictionary<str, obj?>{
			[nameof(PoUser.Level)] = 7,
		});
		T(U.Level == 7, "泛型版 AssignFromDict<PoUser> 同樣寫得回物件");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterBizFlow(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(ITypeInfoSrcExtn), typeof(ITypeInfo)],
			[
				nameof(ITypeInfoSrcExtn.GetInfo),
				nameof(ITypeInfo.WritableMembers),
				nameof(ITypeInfoSrcExtn.AssignFromDict),
			],
			"綜合測試:落庫與回填:"
		);
		reg.Register(nameof(BizRowAndFillBack), BizRowAndFillBack!);
	}
}



