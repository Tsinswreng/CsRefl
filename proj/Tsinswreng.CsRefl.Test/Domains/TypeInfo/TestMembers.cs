using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// Members 契約：成員齊全、順序 = 宣告序（基類在前）、型別/讀寫能力正確。
public partial class TestTypeInfo{
	/// 期望的成員序（見 PoUser 的註釋）。
	private static readonly str[] ExpectOrder = [
		"Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Secret", "Level", "Note",
	];

	/// 對一個來源驗證 PoUser 的成員契約。
	private static void CheckMembers(ITypeInfo Info){
		var T = Assert.IsTrue;

		T(Info.Members.Count == 10, $"應有 10 個成員，實際 {Info.Members.Count}");
		for(var I = 0; I < ExpectOrder.Length; I++){
			var M = Info.Members[I];
			T(M.CodeName == ExpectOrder[I], $"第 {I} 個成員應是 {ExpectOrder[I]}，實際 {M.CodeName}");
		}

		var Age = Info.Members[2];
		T(Age.CodeName == "Age", "Age 應在自分類的第 3 位");
		T(Age.Kind == EMemberKind.Property, "Age 應是 Property");
		T(Age.DeclaredType == typeof(i32), "Age 型別應是 int");
		T(Age.DeclaringType == typeof(PoUser), "Age 聲明型別應是 PoUser");
		T(Age.CanRead && Age.CanWrite, "Age 應可讀可寫");

		var Id = Info.Members[0];
		T(Id.DeclaringType == typeof(PoUserBase), "Id 聲明型別應是基類 PoUserBase（繼承成員在前）");

		var Secret = Info.Members[7];
		T(Secret.CodeName == "Secret", "Secret 應在第 8 位");
		T(Secret.DeclaredType == typeof(str), "Secret 型別應是 string");
		T(Secret.CanRead && !Secret.CanWrite, "Secret 應只讀不可寫");

		var Note = Info.Members[9];
		T(Note.CodeName == "Note", "Note 應在第 10 位");
		T(Note.Kind == EMemberKind.Field || Note.Kind == EMemberKind.Property, "Note 是 [JsonInclude] 字段，反射標 Field、Json 只能標 Property");
		T(Note.CanRead && Note.CanWrite, "Note 應可讀可寫");

		// 靜態成員 / 私有字段 / 索引器都不得進成員表。
		foreach(var M in Info.Members){
			T(M.CodeName is not ("StaticNote" or "Hidden" or "Item"), $"成員 {M.CodeName} 不應出現（靜態/私有/索引器被排除）");
		}
	}

	public void RegisterMembers(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "成員表:"
		);
		var R = reg.Register;
		R("反射來源 成員表契約", async _ => {
			CheckMembers(InfoOf(_refl));
			return null;
		});
		R("Json來源 成員表契約", async _ => {
			CheckMembers(InfoOf(_json));
			return null;
		});
	}
}