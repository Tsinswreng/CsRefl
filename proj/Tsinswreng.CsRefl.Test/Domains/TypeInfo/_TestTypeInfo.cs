using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// 契約測試：同一套斷言分別跑在反射來源與 JsonTypeInfo 來源上，
/// 驗證「兩套實現對外語義一致」是本包的核心設計要求。
///
/// 寫法要點：不為兩套實現各存一個字段，
/// 而是把它們放進同一個 {nameof(ITypeInfoSrc)} 序列裏逐一跑——
/// 用例體因此只寫一遍，且「換實現不改測試」這件事本身就被證明了。
/// 分部文件（TestMembers/TestLookup/TestReadWrite/TestMkInstKind/TestAttrs/TestShadowing）
/// 各自以 RegisterXxx 命名並在下面組裝；它們只放函數實現，聲明都在本文件。
public partial class TestTypeInfo:ITester{
	/// 兩套來源實現（反射、Json 源生成）；用例對它逐個跑同一套斷言。
	private readonly IReadOnlyList<ITypeInfoSrc> _srcs;

	public partial TestTypeInfo(ReflTypeInfoSrc Refl, JsonTypeInfoSrc Json);
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 對某個來源取 PoUser 的元資料（用例都從這裡取，保持測試與來源解耦）。
	private static partial ITypeInfo InfoOf(ITypeInfoSrc Src);

	/// 對一個來源驗證 PoUser 的成員契約（齊全、順序 = 宣告序、能力與官方出口正確）。
	private static partial void CheckMembers(ITypeInfo Info, bool IsRefl);
	/// 註冊成員表用例。
	public partial void RegisterMembers(ITestNode Node);

	/// 對一個來源驗證按名查詢（TryGetMember / GetMember 命中與異常）。
	private static partial void CheckLookup(ITypeInfo Info);
	/// 註冊按名查詢用例。
	public partial void RegisterLookup(ITestNode Node);

	/// 對一個來源驗證成員表與三份子集（Members / ReadableMembers / WritableMembers）。
	private static partial void CheckReadWriteNames(ITypeInfo Info);
	/// 對一個來源驗證 TryGet/TrySet 端到端（造一個實例走完整讀寫）。
	private static partial void CheckReadWriteE2E(ITypeInfo Info);
	/// 註冊讀寫用例。
	public partial void RegisterReadWrite(ITestNode Node);

	/// 對一個來源驗證 PoUser 的無參實例工廠（官方形狀 CreateObject）。
	private static partial void CheckMkInst(ITypeInfo Info);
	/// 對一個來源驗證分類與鍵值型別（Kind 用官方 JsonTypeInfoKind）。
	private static partial void CheckKind(
		ITypeInfo Info, Type T2, JsonTypeInfoKind ExpectKind, Type? ExpectKey, Type? ExpectElem
	);
	/// 註冊實例/分類用例。
	public partial void RegisterMkInstKind(ITestNode Node);

	/// 對一個來源驗證特性可查（用官方 ICustomAttributeProvider + 官方擴展方法）。
	private static partial void CheckAttrs(ITypeInfo Info, bool IsRefl);
	/// 註冊特性用例。
	public partial void RegisterAttrs(ITestNode Node);

	/// 對一個來源驗證同名遮蔽（new）的去重結果。
	private static partial void CheckShadowing(ITypeInfo Info);
	/// 註冊同名遮蔽用例。
	public partial void RegisterShadowing(ITestNode Node);
}
