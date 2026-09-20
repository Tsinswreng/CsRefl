using System.Text.Json.Serialization.Metadata;
using System.Reflection;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// 同名遮蔽（new）契約：成員表按 Name 唯一，成員序仍是「基類在前、同類內宣告序」，
/// 按名查到的是最靠近實例的那份宣告；兩套來源結論一致。
///
/// 實測事實（.NET 10）：Type.GetProperties 本身就不返回被 new 遮蔽的基類屬性，
/// 故兩套來源都不產生重複名。門面的按名去重因此是防禦性的（見 TypeInfoSorter.SortEtDedup），
/// 本用例把它當契約釘住：任何來源若哪天開始給出同名項，這裡會先紅。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	private static partial void CheckShadowing(ITypeInfo Info){
		var T = Assert.IsTrue;

		T(Info.Members.Count == 3, $"應剩 3 個成員，實際 {Info.Members.Count}");
		// 成員序：基類的 Name 在前，然後是派生類自己宣告的 Id、Age。
		T(Member.Name(Info.Members[0]) == "Name"
			&& Member.Name(Info.Members[1]) == "Id"
			&& Member.Name(Info.Members[2]) == "Age",
			$"成員序應是 Name、Id、Age，實際 {string.Join(",", Info.Members.Select(M => Member.Name(M)))}");

		// Id 只出現一次，且是派生類那份宣告。
		var Id = Info.GetMember("Id");
		T(Member.DeclaringType(Id) == typeof(PoUserExt), $"按名查到的 Id 應是派生類宣告，實際 {Member.DeclaringType(Id)?.Name ?? "null"}");

		// 去重後讀寫照常作用在實例上。
		var Ext = new PoUserExt();
		T(Member.TrySet(Id, Ext, 42L) && Ext.Id == 42, "去重後的 Id 應能寫回實例");

		// 名清單不得出現重複鍵。
		T(Info.ReadableNames.Count == 3, $"可讀名應有 3 個，實際 {Info.ReadableNames.Count}");
		T(Info.ReadableNames.Distinct().Count() == Info.ReadableNames.Count, "可讀名不得有重複鍵");
	}

	/// 見聲明處的說明。
	public partial void RegisterShadowing(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUserExt)], [nameof(PoUserExt.Age)], "同名遮蔽:"
		);
		var R = reg.Register;
		foreach(var Src in _srcs){
			R($"{Src.GetType().Name} 遮蔽成員契約", async _ => {
				Assert.IsTrue(Src.TryGetInfo(typeof(PoUserExt), out var Info), $"{Src.GetType().Name} 應能查 PoUserExt");
				CheckShadowing(Info!);
				return null;
			});
		}
	}
}







