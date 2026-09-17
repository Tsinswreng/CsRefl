using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// 操作層（ITypeInfoSrcExtn）測試：以「合成來源」（Json→Reg→Refl）為被測來源，
/// 貼近生產用法——調用方只面對 MergedTypeInfoSrc + 擴展方法。
/// 分部文件的註冊方法以 RegisterXxx 命名並在下面組裝。
public partial class TestFacadeExtn:ITester{
	/// 合成來源（Json 優先、Refl 兜底）。
	private readonly MergedTypeInfoSrc _merged;

	public TestFacadeExtn(MergedTypeInfoSrc Merged){
		_merged = Merged;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
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