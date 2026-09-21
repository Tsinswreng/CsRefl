using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 綜合測試：演示本庫的用法。這份檔案就是用法示例。
///
/// 用例一律站在調用方那一側寫：調用方手上是一個 ITypeInfoSrc（門面），
/// 他就是從這個門面調庫的，裏面是源生成、反射還是合成與他無關。
/// 來源是每個用例自己建出來的，起點就擺在用例開頭——不用類字段，也沒有自己封裝的 helper。
///
/// 分部文件按【庫的對外層級】劃分，一層一檔，檔內把同一操作的兩種寫法並排：
/// + TestFacade——門面層 ITypeInfoSrcExtn（調用方的起點，GetInfo/GetMember/TryGet…）；
/// + TestTypeInfo——型別元資料層 ITypeInfo／ITypeInfoExtn（拿到 Info 之後的讀寫）；
/// + TestMember——成員層 IMemberInfo（成員物件是官方成員物件的配接器，這層沒有泛型版）；
/// + TestInstDict——視圖層 IInstDict／InstDict（ToInstDict 的兩種寫法與讀寫口徑）；
/// + TestBizFlow——端到端：把上面幾層串成一段業務流程（落庫與回填）。
///
/// 兩種寫法並排是刻意的：Type 顯式版給「型別運行期才知道」的場合（例如按 O.GetType() 查），
/// 泛型版給「型別編譯期已知」的場合（DAM 掛在 T 上，剪裁器看得見需求）。
/// 兩條路並存，不是替代。
public partial class TestSynthesis:ITester{
	/// 組裝本域用例。
	public partial ITestNode RegisterTestsInto(ITestNode? Node);

	/// 層一：門面層——起點與各操作（顯式型別版與泛型版並排）。
	public partial Task<nil> FacadeOps(obj? O);
	/// 註冊層一用例。
	public partial void RegisterFacade(ITestNode Node);

	/// 層二：型別元資料層——成員表、名清單、按名讀寫、實例工廠。
	public partial Task<nil> TypeInfoOps(obj? O);
	/// 註冊層二用例。
	public partial void RegisterTypeInfo(ITestNode Node);

	/// 層三：成員層——成員本體（官方物件）上的操作。
	public partial Task<nil> MemberOps(obj? O);
	/// 註冊層三用例。
	public partial void RegisterMember(ITestNode Node);

	/// 層四：視圖層——把物件當字典用。
	public partial Task<nil> DictView(obj? O);
	/// 註冊層四用例。
	public partial void RegisterInstDict(ITestNode Node);

	/// 端到端：落庫與回填。
	public partial Task<nil> BizRowAndFillBack(obj? O);
	/// 註冊端到端用例。
	public partial void RegisterBizFlow(ITestNode Node);
}

