using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// TestInstDict 的骨架實現：建構子、工廠、註冊組裝。
/// 只放函數實現：字段與聲明在 _TestInstDict.cs。
public partial class TestInstDict{
	/// 見聲明處的說明。
	public partial TestInstDict(MergedTypeInfoSrc Merged){
		_merged = Merged;
	}

	/// 見聲明處的說明。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterShape(Node);
		RegisterReadWrite(Node);
		RegisterErrors(Node);
		return Node;
	}

	/// 見聲明處的說明。
	private static partial PoUser MakeUser(){
		return new PoUser{ Id = 1, Name = "小明", Age = 26, Married = true, Level = 3 };
	}

	/// 見聲明處的說明。
	private partial IInstViewDict MakeDict(PoUser User){
		return _merged.ToInstViewDict(User);
	}
}
