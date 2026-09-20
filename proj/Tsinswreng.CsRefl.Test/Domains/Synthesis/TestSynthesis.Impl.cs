using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// TestSynthesis 的骨架實現：只負責組裝用例。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。新增用例時在這裡登記一行。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		// 用例之間互不相干，可並行；組裝順序即閱讀順序。
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterMkSrc(Node);
		RegisterBizFlow(Node);
		RegisterDictView(Node);
		RegisterLookup(Node);
		return Node;
	}
}
