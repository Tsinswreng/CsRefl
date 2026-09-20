using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl.Test.Domains.FacadeExtn;
using Tsinswreng.CsRefl.Test.Domains.InstDict;
using Tsinswreng.CsRefl.Test.Domains.Src;
using Tsinswreng.CsRefl.Test.Domains.Synthesis;
using Tsinswreng.CsRefl.Test.Domains.TypeInfo;
namespace Tsinswreng.CsRefl.Test;

public class CsReflTestMgr:DiEtTestMgr{
	public static CsReflTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestTypeInfo>();
		this.RegisterTester<TestFacadeExtn>();
		this.RegisterTester<TestInstDict>();
		this.RegisterTester<TestTypeInfoReg>();
		this.RegisterTester<TestMergedTypeInfoSrc>();
		this.RegisterTester<TestSynthesis>();
		return Node;
	}
}
