using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.FacadeExtn;

/// 操作層（ITypeInfoSrcExtn）測試：以「合成來源」（Json→Reg→Refl）為被測來源，
/// 貼近生產用法——調用方只面對 MergedTypeInfoSrc + 擴展方法。
/// 分部文件（TestGetMember/TestTryGetTrySet/TestToInstDict/TestAssignFromDict）
/// 各自以 RegisterXxx 命名並在下面組裝；它們只放函數實現，聲明都在本文件。
public partial class TestFacadeExtn:ITester{
	/// 合成來源（Json 優先、Refl 兜底）。
	private readonly MergedTypeInfoSrc _merged;

	public partial TestFacadeExtn(MergedTypeInfoSrc Merged);
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 註冊 GetMember 用例（命中/未知成員/未註冊型別）。
	public partial void RegisterGetMember(ITestNode Node);
	/// 註冊 TryGet/TrySet 用例。
	public partial void RegisterTryGetTrySet(ITestNode Node);
	/// 註冊 ToInstDict 用例。
	public partial void RegisterToInstDict(ITestNode Node);
	/// 註冊 AssignFromDict 用例。
	public partial void RegisterAssignFromDict(ITestNode Node);
}
