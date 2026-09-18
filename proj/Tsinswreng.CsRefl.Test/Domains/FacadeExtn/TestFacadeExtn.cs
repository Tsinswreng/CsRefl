using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// TestFacadeExtn 的骨架實現：建構子與註冊組裝。
/// 只放函數實現：字段與聲明在 _TestFacadeExtn.cs。
public partial class TestFacadeExtn{
	/// 見聲明處的說明。
	public partial TestFacadeExtn(MergedTypeInfoSrc Merged){
		_merged = Merged;
	}

	/// 見聲明處的說明。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterGetMember(Node);
		RegisterTryGetTrySet(Node);
		RegisterToInstDict(Node);
		RegisterAssignFromDict(Node);
		return Node;
	}
}