using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 層一：門面層 ITypeInfoSrcExtn——調用方的起點，各操作都在這一層進來。
/// 排法：先顯式型別版（型別運行期才知道時用），再把泛型版集中在檔尾（型別編譯期已知時用）。
/// 兩條路並存，不是替代。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> FacadeOps(obj? O){
		var T = Assert.IsTrue;

		// 起點：調用方自己把門面建出來。生產用合成來源（源生成優先、反射兜底）。
		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);
		var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
		var ReflOnly = new ReflTypeInfoSrc();

		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		// ================= 顯式型別版 =================

		// GetInfo：查不到就拋；沒把握時用 TryGetInfo。
		var Info1 = Src.GetInfo(typeof(PoUser));
		T(Info1.Type == typeof(PoUser), "GetInfo 應取到 PoUser 的元資料");

		// 換來源：源生成只認掛過 [JsonSerializable] 的型別；反射來源兜底；合成來源兩頭都接。
		T(JsonOnly.TryGetInfo(typeof(PoUser), out _), "掛過的型別：源生成查得到");
		T(!JsonOnly.TryGetInfo(typeof(PoNoCtor), out _), "沒掛的型別：源生成查不到");
		T(ReflOnly.TryGetInfo(typeof(PoNoCtor), out _), "反射來源不需要註冊任何型別");
		T(Src.GetInfo(typeof(PoNoCtor)).Type == typeof(PoNoCtor), "合成來源：沒掛的型別由反射兜住");
		T(ReferenceEquals(ReflTypeInfoSrc.Inst, ReflTypeInfoSrc.Inst), "反射來源有默認單例");

		// GetMember / TryGetMember。
		var M1 = Src.GetMember(typeof(PoUser), nameof(PoUser.Age));
		T(MemberExtn.Name(M1) == nameof(PoUser.Age), "GetMember 應取到 Age 成員");
		T(Src.TryGetMember(typeof(PoUser), nameof(PoUser.Level), out _), "TryGetMember 命中");
		T(!Src.TryGetMember(typeof(PoUser), "NoSuch", out _), "TryGetMember 未命中返回 false");

		// TryGet / TrySet。
		T(Src.TryGet(typeof(PoUser), U, nameof(PoUser.Age), out var V1) && (i32)V1! == 26, "TryGet 讀到 26");
		T(Src.TrySet(typeof(PoUser), U, nameof(PoUser.Age), 27), "TrySet 寫得進");
		T(U.Age == 27, "寫完物件應變成 27");

		// 失敗分叉：未知名字／只寫成員／只讀成員。
		T(!Src.TryGet(typeof(PoUser), U, "NoSuch", out _), "未知名字返回 false");
		T(!Src.TryGet(typeof(PoUser), U, nameof(PoUser.Token), out _), "只寫成員讀不到");
		T(!Src.TrySet(typeof(PoUser), U, nameof(PoUser.Secret), "x") && U.Secret == "s", "只讀成員寫不進且不改值");

		// GetInfo 查不到就拋。
		var Threw = false;
		try{
			JsonOnly.GetInfo(typeof(PoNoCtor));
		}catch(KeyNotFoundException){
			Threw = true;
		}
		T(Threw, "GetInfo 查不到應拋 KeyNotFoundException");

		// ================= 泛型版（型別編譯期已知）=================
		// 差別只有兩點：少寫一個 typeof；DAM 掛在 T 上，剪裁器看得見「這個型別需要成員元數據」。

		var Info2 = Src.GetInfo<PoUser>();
		T(Info2.Type == typeof(PoUser), "GetInfo<PoUser> 應取到 PoUser 的元資料");
		T(ReferenceEquals(Info1, Info2), "同一來源上兩種寫法取到同一份實例（來源自己緩存）");

		var M2 = Src.GetMember<PoUser>(nameof(PoUser.Age));
		T(MemberExtn.Name(M2) == nameof(PoUser.Age), "GetMember<PoUser> 應取到 Age 成員");
		T(Src.TryGetMember<PoUser>(nameof(PoUser.Level), out _), "TryGetMember<T> 命中");
		T(!Src.TryGetMember<PoUser>("NoSuch", out _), "TryGetMember<T> 未命中返回 false");

		T(Src.TryGet<PoUser>(U, nameof(PoUser.Age), out var V2) && (i32)V2! == 27, "TryGet<T> 讀到 27");
		T(Src.TrySet<PoUser>(U, nameof(PoUser.Age), 28), "TrySet<T> 寫得進");
		T(U.Age == 28, "寫完物件應變成 28");

		T(!Src.TryGet<PoUser>(U, "NoSuch", out _), "泛型版：未知名字返回 false");
		T(!Src.TryGet<PoUser>(U, nameof(PoUser.Token), out _), "泛型版：只寫成員讀不到");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterFacade(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(ITypeInfoSrcExtn), typeof(ITypeInfoSrc)],
			[nameof(ITypeInfoSrcExtn.GetInfo), nameof(ITypeInfoSrcExtn.TryGet)],
			"綜合測試:門面層:"
		);
		reg.Register(nameof(FacadeOps), FacadeOps!);
	}
}
