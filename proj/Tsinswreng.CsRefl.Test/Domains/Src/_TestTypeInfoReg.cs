using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// TypeInfoReg（手寫註冊表）測試。
/// 注意：測試用本地 new 的實例，不碰 DI 裏那個被 MergedTypeInfoSrc 引用的單例，
/// 避免並行測試互相污染。
public partial class TestTypeInfoReg:ITester{
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = true;
		RegisterAddRemoveEnum(Node);
		return Node;
	}
}