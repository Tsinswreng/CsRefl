using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// TypeInfoReg（手寫註冊表）測試。
/// 注意：測試用本地 new 的實例，不碰 DI 裏那個被 MergedTypeInfoSrc 引用的單例，
/// 避免並行測試互相污染。
/// 本文件只放聲明；實現見 TestTypeInfoReg.Impl.cs。
public partial class TestTypeInfoReg:ITester{
	/// 組裝本域用例（增刪與列舉）。
	public partial ITestNode RegisterTestsInto(ITestNode? Node);
	/// 註冊註冊表的增刪與列舉用例。
	public partial void RegisterAddRemoveEnum(ITestNode Node);
}