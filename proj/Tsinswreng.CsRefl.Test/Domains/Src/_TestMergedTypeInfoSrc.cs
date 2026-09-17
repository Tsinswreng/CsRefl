using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// MergedTypeInfoSrc（多來源合成）測試。
/// DI 註冊的合成來源是 Json→Reg→Refl，這裡直接用它驗證優先級；
/// 需要控制優先級/列舉語義的場景就近 new 一個本地合成，不污染共享實例。
public partial class TestMergedTypeInfoSrc:ITester{
	/// DI 的合成來源（Json→Reg→Refl）。
	private readonly MergedTypeInfoSrc _merged;
	/// Json 來源（對照）。
	private readonly JsonTypeInfoSrc _json;
	/// 反射來源（對照）。
	private readonly ReflTypeInfoSrc _refl;

	public TestMergedTypeInfoSrc(MergedTypeInfoSrc Merged, JsonTypeInfoSrc Json, ReflTypeInfoSrc Refl){
		_merged = Merged;
		_json = Json;
		_refl = Refl;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = true;
		RegisterPriority(Node);
		RegisterRegistered(Node);
		return Node;
	}
}