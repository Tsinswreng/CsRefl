using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.InstDict;

/// InstDict（淺字典視圖）測試：以合成來源建視圖。
/// 分部文件（TestShape/TestReadWrite/TestErrors）各自以 RegisterXxx 命名並在下面組裝；
/// 它們只放函數實現，聲明都在本文件。
public partial class TestInstDict:ITester{
	/// 合成來源（Json 優先、Refl 兜底）。
	private readonly MergedTypeInfoSrc _merged;

	public partial TestInstDict(MergedTypeInfoSrc Merged);
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 造一個標準的測試物件。
	private static partial PoUser MakeUser();
	/// 用合成來源為該物件建視圖。
	private partial IInstDict MakeDict(PoUser User);

	/// 註冊視圖形狀用例（鍵序/Values/IsReadOnly/Keys）。
	public partial void RegisterShape(ITestNode Node);
	/// 註冊視圖讀寫用例（索引器/ContainsKey/TryGetValue/枚舉）。
	public partial void RegisterReadWrite(ITestNode Node);
	/// 註冊視圖異常路徑用例（形狀固定/未知鍵/只讀成員）。
	public partial void RegisterErrors(ITestNode Node);
}