using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// JsonName 契約：Json 來源的 JsonName == CodeName（命名策略為 null 的前提）；
/// 反射來源沒有 JSON 名，JsonName 為 null。
public partial class TestTypeInfo{
	public void RegisterJsonName(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "JsonName:"
		);
		var R = reg.Register;
		R("反射來源 JsonName 為 null", async _ => {
			var T = Assert.IsTrue;
			var Info = InfoOf(_refl);
			var Age = Info.GetMember("Age");
			T(Age.JsonName is null, "反射來源沒有 JSON 名，JsonName 應為 null");
			T(Age.CodeName == "Age", "CodeName 應是 Age");
			return null;
		});
		R("Json來源 JsonName 等於 CodeName", async _ => {
			var T = Assert.IsTrue;
			var Info = InfoOf(_json);
			var Age = Info.GetMember("Age");
			T(Age.JsonName == "Age", $"JsonName 應等於 CodeName（默認命名策略），實際 {Age.JsonName}");
			T(Age.CodeName == "Age", "CodeName 應是 Age");
			T(Age.JsonName == Age.CodeName, "JsonName 與 CodeName 應相同");
			return null;
		});
	}
}