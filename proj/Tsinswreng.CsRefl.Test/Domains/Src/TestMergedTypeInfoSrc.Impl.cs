using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// TestMergedTypeInfoSrc 的骨架實現：建構子與註冊組裝。
/// 只放函數實現：字段與聲明在 TestMergedTypeInfoSrc.cs。
public partial class TestMergedTypeInfoSrc{
	/// 見聲明處的說明。
	public partial TestMergedTypeInfoSrc(MergedTypeInfoSrc Merged, JsonTypeInfoSrc Json, ReflTypeInfoSrc Refl){
		_merged = Merged;
		_json = Json;
		_refl = Refl;
	}

	/// 見聲明處的說明。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = true;
		RegisterPriority(Node);
		RegisterRegistered(Node);
		return Node;
	}
}
