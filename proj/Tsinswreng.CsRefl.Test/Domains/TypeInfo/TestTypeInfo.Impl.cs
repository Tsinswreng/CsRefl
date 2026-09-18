using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// TestTypeInfo 的骨架實現：建構子、註冊組裝、取元資料的小工具。
/// 只放函數實現：字段與聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	public partial TestTypeInfo(ReflTypeInfoSrc Refl, JsonTypeInfoSrc Json){
		_refl = Refl;
		_json = Json;
	}

	/// 見聲明處的說明。新增用例時在這裡登記一行。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterMembers(Node);
		RegisterLookup(Node);
		RegisterReadWrite(Node);
		RegisterMkInstKind(Node);
		RegisterAttrs(Node);
		RegisterShadowing(Node);
		return Node;
	}

	/// 見聲明處的說明。
	private static partial ITypeInfo InfoOf(ITypeInfoSrc Src){
		Assert.IsTrue(Src.TryGetInfo(typeof(PoUser), out var Info), "PoUser 應在來源上可查");
		return Info!;
	}
}