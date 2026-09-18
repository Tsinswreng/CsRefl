using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// RegisteredTypes 語義：全部來源可列舉才返回並集，任一來源不支持列舉則返回 null。
/// 函數實現文件；聲明在 TestMergedTypeInfoSrc.cs。
public partial class TestMergedTypeInfoSrc{
	/// 見聲明處的說明。
	public partial void RegisterRegistered(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestMergedTypeInfoSrc), [typeof(MergedTypeInfoSrc)], [nameof(MergedTypeInfoSrc.RegisteredTypes)], "列舉語義:"
		);
		var R = reg.Register;

		R("全可列舉時為並集", async _ => {
			var T = Assert.IsTrue;
			var A = new TypeInfoReg();
			A.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			var B = new TypeInfoReg();
			B.Add(typeof(PoColor), new ReflTypeInfo(typeof(PoColor)));
			B.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			var Types = new MergedTypeInfoSrc(A, B).RegisteredTypes!;
			T(Types.Count == 2, $"並集應去重，實際 {Types.Count}");
			T(Types.Contains(typeof(PoUser)) && Types.Contains(typeof(PoColor)), "並集應含兩個型別");
			return null;
		});

		R("任一來源不支持列舉則為 null", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			Table.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			// Json 來源 RegisteredTypes 恒為 null → 合成也 null。
			T(new MergedTypeInfoSrc(Table, _json).RegisteredTypes is null, "含 Json 來源的合成列舉應為 null");
			T(_merged.RegisteredTypes is null, "DI 合成（含 Json）列舉應為 null");
			return null;
		});
	}
}