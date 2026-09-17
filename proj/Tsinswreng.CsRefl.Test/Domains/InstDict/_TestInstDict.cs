using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// InstDict（淺字典視圖）測試：以合成來源建視圖。
/// 分部文件的註冊方法以 RegisterXxx 命名並在下面組裝。
public partial class TestInstDict:ITester{
	/// 合成來源（Json 優先、Refl 兜底）。
	private readonly MergedTypeInfoSrc _merged;

	public TestInstDict(MergedTypeInfoSrc Merged){
		_merged = Merged;
	}

	/// 造一個標準的測試物件。
	private static PoUser MakeUser(){
		return new PoUser{ Id = 1, Name = "小明", Age = 26, Married = true, Level = 3 };
	}

	/// 建視圖。
	private IInstDict MakeDict(PoUser User){
		return _merged.ToInstDict(User);
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterShape(Node);
		RegisterReadWrite(Node);
		RegisterErrors(Node);
		return Node;
	}
}