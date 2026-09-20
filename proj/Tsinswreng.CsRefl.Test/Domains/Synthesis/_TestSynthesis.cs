using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 綜合測試：演示本庫的用法。這份檔案就是用法示例。
///
/// 用例一律站在調用方那一側寫：調用方手上是一個 <see cref="ITypeInfoSrc"/>（門面），
/// 他就是從這個門面調庫的，裏面是源生成、反射還是合成，與他無關。
/// 因此用例裏不會出現「哪套實現」這種話，也不把來源存進類字段——
/// 來源是每個用例自己建出來的，起點就擺在用例開頭。
///
/// 每一行都是庫的真 API 調用：<see cref="ITypeInfoSrcExtn.GetInfo"/>、
/// <see cref="ITypeInfoSrcExtn.TryGet"/>、<see cref="ITypeInfoSrcExtn.TrySet"/>、
/// <see cref="ITypeInfoSrcExtn.AssignFromDict"/>、<see cref="ITypeInfoSrcExtn.ToInstDict"/>、
/// <see cref="ITypeInfo.WritableNames"/>、<see cref="ITypeInfoExtn.TryGet"/>、
/// <see cref="ITypeInfo.GetMember"/>、<see cref="MemberExtn"/>。
/// 用例裏沒有自己封裝的方法（沒有 `MkUser` 這類 helper，物件就地 `new`）。
///
/// 四個用例：
/// 一、起點——門面怎麼建（生產用合成、只用源生成、只用反射）；
/// 二、落庫與回填——一段完整業務流程怎麼寫；
/// 三、物件當字典用——表單綁定與可視化；
/// 四、按名讀寫的邊界——只讀、只寫、未知名字分別會怎樣。
///
/// 分部文件（TestMkSrc／TestBizFlow／TestDictView／TestLookup）各自以 RegisterXxx 命名並在下面組裝；
/// 它們只放函數實現，聲明都在本文件。
public partial class TestSynthesis:ITester{
	/// 組裝本域用例。
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 用法一：門面從哪來。
	public partial Task<nil> MkSrc(obj? O);
	/// 註冊用法一。
	public partial void RegisterMkSrc(ITestNode Node);

	/// 用法二：把業務物件落成一行、再從字典回填。
	public partial Task<nil> BizRowAndFillBack(obj? O);
	/// 註冊用法二。
	public partial void RegisterBizFlow(ITestNode Node);

	/// 用法三：把物件當字典用。
	public partial Task<nil> ObjectAsDict(obj? O);
	/// 註冊用法三。
	public partial void RegisterDictView(ITestNode Node);

	/// 用法四：按名讀寫的邊界。
	public partial Task<nil> ReadWriteEdges(obj? O);
	/// 註冊用法四。
	public partial void RegisterLookup(ITestNode Node);

	/// 用法五：泛型入口——型別靜態已知時用泛型版（與非泛型版並存，不是替代）。
	public partial Task<nil> GenericEntry(obj? O);
	/// 註冊用法五。
	public partial void RegisterGenericEntry(ITestNode Node);
}

