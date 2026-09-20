using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 層二：型別元資料層 ITypeInfo／ITypeInfoExtn——已經拿到 Info 之後的讀寫。
/// 排法：先顯式實例型別版，再把泛型版集中在檔尾。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> TypeInfoOps(obj? O){
		var T = Assert.IsTrue;

		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);
		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		var Info = Src.GetInfo(typeof(PoUser));

		// 成員表與名清單：順序即契約序（基類在前、同類內宣告序）。
		// 這兩者是屬性，按實例緩存，沒有泛型/非泛型之分。
		T(Info.Members.Count == 11, $"成員應有 11 個，實際 {Info.Members.Count}");
		T(Info.WritableNames.Count == 9, $"可寫成員應有 9 個，實際 {Info.WritableNames.Count}");
		T(Info.ReadableNames.Count == 10, $"可讀成員應有 10 個，實際 {Info.ReadableNames.Count}");
		T(Info.ReadableNames.Contains(nameof(PoUser.Secret)) && !Info.WritableNames.Contains(nameof(PoUser.Secret))
			, "只讀的 Secret 在可讀清單、不在可寫清單");

		// 按名查成員：TryGetMember / GetMember。
		T(Info.TryGetMember(nameof(PoUser.Age), out var MAge), "TryGetMember 命中");
		T(Member.Name(Info.GetMember(nameof(PoUser.Age))) == nameof(PoUser.Age), "GetMember 取到 Age");
		T(!Info.TryGetMember("NoSuch", out _), "未知名字返回 false");

		// 實例工廠：判據就是 CreateObject 是否為 null。
		T(Info.CanMkInst, "PoUser 應可建無參實例");
		T(Info.MkInst() is PoUser, "MkInst 應產生 PoUser");
		T(!Src.GetInfo(typeof(PoNoCtor)).CanMkInst, "PoNoCtor 只有帶參構造函數，不可建");

		// ================= 顯式實例型別版 =================

		T(Info.TryGet(U, nameof(PoUser.Age), out var V1) && (i32)V1! == 26, "TryGet 讀到 26");
		T(Info.TrySet(U, nameof(PoUser.Age), 31), "TrySet 寫得進");
		T(U.Age == 31, "寫完物件應變成 31");
		T(!Info.TrySet(U, nameof(PoUser.Secret), "x") && U.Secret == "s", "只讀成員寫不進且不改值");
		T(!Info.TryGet(U, nameof(PoUser.Token), out _), "只寫成員讀不到");

		// ================= 泛型版 =================

		T(Info.TryGet<PoUser>(U, nameof(PoUser.Age), out var V2) && (i32)V2! == 31, "TryGet<T> 讀到 31");
		T(Info.TrySet<PoUser>(U, nameof(PoUser.Age), 32), "TrySet<T> 寫得進");
		T(U.Age == 32, "寫完物件應變成 32");
		T(!Info.TrySet<PoUser>(U, nameof(PoUser.Secret), "x") && U.Secret == "s", "泛型版：只讀成員寫不進");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterTypeInfo(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(ITypeInfo), typeof(ITypeInfoExtn)],
			[nameof(ITypeInfo.TryGetMember), nameof(ITypeInfoExtn.TryGet)],
			"綜合測試:型別元資料層:"
		);
		reg.Register(nameof(TypeInfoOps), TypeInfoOps!);
	}
}


