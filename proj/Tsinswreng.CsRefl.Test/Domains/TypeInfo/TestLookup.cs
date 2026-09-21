using System.Text.Json.Serialization.Metadata;
using System.Reflection;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// Lookup 契約：TryGetMember/GetMember 的命中與異常行為。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	private static partial void CheckLookup(ITypeInfo Info){
		var T = Assert.IsTrue;

		T(Info.TryGetMember("Age", out var Age), "TryGetMember 應命中 Age");
		T(Age!.PropertyType == typeof(i32), "命中成員的型別應正確");

		T(!Info.TryGetMember("NoSuch", out var Miss), "TryGetMember 對未知成員應返回 false");
		T(Miss is null, "未命中時 out 應為 null");

		var ByGet = Info.GetMember("Age");
		T(ReferenceEquals(Age, ByGet), "GetMember 與 TryGetMember 應返回同一實例（緩存生效）");

		var Threw = false;
		try{
			Info.GetMember("NoSuch");
		}
		catch(KeyNotFoundException E){
			Threw = true;
			T(E.Message.Contains("Age"), $"異常訊息應含可用鍵清單，實際：{E.Message}");
		}
		T(Threw, "GetMember 對未知成員應拋 KeyNotFoundException");
	}

	/// 見聲明處的說明。
	public partial void RegisterLookup(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "按名查詢:"
		);
		var R = reg.Register;
		foreach(var Src in _srcs){
			R($"{Src.GetType().Name} 查詢契約", async _ => {
				CheckLookup(InfoOf(Src));
				return null;
			});
		}
	}
}










