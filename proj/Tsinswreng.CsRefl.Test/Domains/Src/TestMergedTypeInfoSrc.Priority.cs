using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// 合成優先級：第一個答「已知」的來源勝出，以及 params 的防禦拷貝。
/// 函數實現文件；聲明在 TestMergedTypeInfoSrc.cs。
public partial class TestMergedTypeInfoSrc{
	/// 見聲明處的說明。
	public partial void RegisterPriority(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestMergedTypeInfoSrc), [typeof(MergedTypeInfoSrc)], [nameof(MergedTypeInfoSrc.TryGetInfo)], "優先級:"
		);
		var R = reg.Register;

		R("已註冊型別走 Json 來源", async _ => {
			var T = Assert.IsTrue;
			T(_merged.TryGetInfo(typeof(PoUser), out var Info), "PoUser 合成查詢應命中");
			T(_json.TryGetInfo(typeof(PoUser), out var JsonInfo), "Json 來源本身應命中");
			T(ReferenceEquals(Info, JsonInfo), "合成結果應就是 Json 來源的那個實例（Json 優先）");
			return null;
		});

		R("Json 不認的型別走 Refl 兜底", async _ => {
			var T = Assert.IsTrue;
			T(_merged.TryGetInfo(typeof(PoNoCtor), out var Info), "PoNoCtor 合成查詢應命中（Refl 兜底）");
			T(_refl.TryGetInfo(typeof(PoNoCtor), out var ReflInfo), "反射來源本身應命中");
			T(ReferenceEquals(Info, ReflInfo), "合成結果應就是 Refl 來源的那個實例");
			return null;
		});

		R("Reg 在 Refl 之前才有效", async _ => {
			var T = Assert.IsTrue;
			// Reg 進合成時必須排在 Refl 前面，否則全能反射會蓋掉手工註冊。
			var Table = new TypeInfoReg();
			var HandInfo = new ReflTypeInfo(typeof(PoColor));
			Table.Add(typeof(PoColor), HandInfo);
			var Merged = new MergedTypeInfoSrc(Table, _refl);
			T(Merged.TryGetInfo(typeof(PoColor), out var Info), "Reg 在前應命中");
			T(ReferenceEquals(Info, HandInfo), "應命中 Reg 裏手動塞的實例而非 Refl 現場建的");
			return null;
		});

		R("空來源不允許", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_ = new MergedTypeInfoSrc();
			}
			catch(ArgumentException){
				Threw = true;
			}
			T(Threw, "無來源的 Merged 應拋 ArgumentException");
			return null;
		});

		R("來源為 null 不允許", async _ => {
			var T = Assert.IsTrue;
			var Threw = false;
			try{
				_ = new MergedTypeInfoSrc(_json, null!);
			}
			catch(ArgumentNullException){
				Threw = true;
			}
			T(Threw, "含 null 來源的 Merged 應拋 ArgumentNullException");
			return null;
		});

		R("params 數組被防禦拷貝", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			var HandInfo = new ReflTypeInfo(typeof(PoColor));
			Table.Add(typeof(PoColor), HandInfo);
			var Sources = new ITypeInfoSrc[]{ Table, _refl };
			var Merged = new MergedTypeInfoSrc(Sources);
			// 修前 _sources 直接存調用方數組：改動它就能改掉合成來源的優先級。
			Sources[0] = _refl;
			T(Merged.TryGetInfo(typeof(PoColor), out var Info), "改動原數組後合成查詢仍應命中");
			T(ReferenceEquals(Info, HandInfo), "合成應仍按原優先級命中 Reg 的實例");
			return null;
		});
	}
}