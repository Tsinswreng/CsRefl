using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// MergedTypeInfoSrc（多來源合成）測試。
/// DI 註冊的合成來源是 Json→Reg→Refl，這裡直接用它驗證優先級；
/// 需要控制優先級/列舉語義的場景就近 new 一個本地合成，不污染共享實例。
///
/// 本文件只放聲明（字段 / 構造子 / 方法簽名與註釋）；
/// 實現見 TestMergedTypeInfoSrc.Impl.cs（骨架與註冊組裝）與
/// TestMergedTypeInfoSrc.Registered.cs（列舉語義用例）。
public partial class TestMergedTypeInfoSrc:ITester{
	/// DI 的合成來源（Json→Reg→Refl）。
	private readonly MergedTypeInfoSrc _merged;
	/// Json 來源（對照）。
	private readonly JsonTypeInfoSrc _json;
	/// 反射來源（對照）。
	private readonly ReflTypeInfoSrc _refl;

	/// 注入三個來源：合成的、Json 的、反射的。
	public partial TestMergedTypeInfoSrc(MergedTypeInfoSrc Merged, JsonTypeInfoSrc Json, ReflTypeInfoSrc Refl);
	/// 組裝本域用例：優先級與列舉語義。
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 註冊來源優先級用例（Json 優先、Refl 兜底、Reg 位置、空來源）。
	public partial void RegisterPriority(ITestNode Node);
	/// 註冊列舉（RegisteredTypes）語義用例。
	public partial void RegisterRegistered(ITestNode Node);
}
