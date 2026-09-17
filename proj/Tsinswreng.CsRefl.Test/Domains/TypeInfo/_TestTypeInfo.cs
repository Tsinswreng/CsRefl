using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// 契約測試：同一套斷言分別跑在反射來源與 JsonTypeInfo 來源上，
/// 驗證「兩套實現對外語義一致」是本包的核心設計要求。
/// 分部文件的註冊方法以 RegisterXxx 命名並在下面組裝。
public partial class TestTypeInfo:ITester{
	/// 反射來源。
	private readonly ReflTypeInfoSrc _refl;
	/// JsonTypeInfo 來源。
	private readonly JsonTypeInfoSrc _json;

	public TestTypeInfo(ReflTypeInfoSrc Refl, JsonTypeInfoSrc Json){
		_refl = Refl;
		_json = Json;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterMembers(Node);
		RegisterLookup(Node);
		RegisterReadWrite(Node);
		RegisterMkInstKind(Node);
		RegisterAttrs(Node);
		RegisterJsonName(Node);
		return Node;
	}

	/// 對某個來源取 PoUser 的元資料（用例都從這裡取，保持測試與來源解耦）。
	private static ITypeInfo InfoOf(ITypeInfoSrc Src){
		Assert.IsTrue(Src.TryGetInfo(typeof(PoUser), out var Info), "PoUser 應在來源上可查");
		return Info!;
	}
}