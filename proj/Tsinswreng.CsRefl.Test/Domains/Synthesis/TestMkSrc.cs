using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 用法一：門面從哪來——調用方自己把它建出來。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> MkSrc(obj? O){
		var T = Assert.IsTrue;

		// 生產用它：合成的門面，源生成優先、反射兜底。
		// 型別掛了 [JsonSerializable] 的走源生成（讀寫是官方委託、零反射），沒掛的落到反射。
		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);
		T(Src.GetInfo(typeof(PoUser)).Type == typeof(PoUser), "掛過的型別應查得到");
		T(Src.GetInfo(typeof(PoNoCtor)).Type == typeof(PoNoCtor), "沒掛的型別應由反射兜住");

		// 只用源生成：查得到的只有掛過 [JsonSerializable] 的那些型別（清單見 TestJsonCtx）。
		var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
		T(JsonOnly.TryGetInfo(typeof(PoUser), out _), "掛過的型別：查得到");
		T(!JsonOnly.TryGetInfo(typeof(PoNoCtor), out _), "沒掛的型別：查不到");

		// 只用反射：不必事先註冊任何型別（NativeAOT 下要求成員元數據被保住）。
		var ReflOnly = new ReflTypeInfoSrc();
		T(ReflOnly.TryGetInfo(typeof(PoNoCtor), out _), "反射來源不需要註冊");
		// 不接 DI 的場合可以直接用它的默認單例。
		T(ReferenceEquals(ReflTypeInfoSrc.Inst, ReflTypeInfoSrc.Inst), "反射來源有默認單例");

		// 查不到就拋的是 GetInfo；名字/型別沒把握時用 TryGetInfo。
		var Threw = false;
		try{
			JsonOnly.GetInfo(typeof(PoNoCtor));
		}catch(KeyNotFoundException){
			Threw = true;
		}
		T(Threw, "GetInfo 查不到應拋 KeyNotFoundException");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterMkSrc(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(MergedTypeInfoSrc), typeof(JsonTypeInfoSrc), typeof(ReflTypeInfoSrc)],
			[nameof(ITypeInfoSrc.TryGetInfo)],
			"綜合測試:門面起點:"
		);
		reg.Register(nameof(MkSrc), MkSrc!);
	}
}
