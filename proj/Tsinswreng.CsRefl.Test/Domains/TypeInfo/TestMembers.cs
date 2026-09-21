using System.Text.Json.Serialization.Metadata;
using System.Reflection;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// Members 契約：成員齊全、順序 = 宣告序（基類在前）、型別/讀寫能力正確，
/// 且官方出口（Member/Json/Get/Set/AttributeProvider）都拿得到真東西。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 期望的成員序（見 PoUser 的註釋）。
	private static readonly str[] ExpectOrder = [
		"Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Secret", "Level", "Token", "Note",
	];

	/// 見聲明處的說明。
	private static partial void CheckMembers(ITypeInfo Info, bool IsRefl){
		var T = Assert.IsTrue;

		T(Info.Members.Count == 11, $"應有 11 個成員，實際 {Info.Members.Count}");
		for(var I = 0; I < ExpectOrder.Length; I++){
			var M = Info.Members[I];
			T(M.Name == ExpectOrder[I], $"第 {I} 個成員應是 {ExpectOrder[I]}，實際 {M.Name}");
		}

		var Age = Info.Members[2];
		T(Age.Name == "Age", "Age 應在自分類的第 3 位");
		T(Age.PropertyType == typeof(i32), "Age 型別應是 int");
		T(Age.DeclaringType == typeof(PoUser), "Age 聲明型別應是 PoUser");
		T(Age.CanRead && Age.CanWrite, "Age 應可讀可寫");
		// 可讀/可寫與官方委託同一判據（官方 JsonPropertyInfo 就是用 Get/Set 表示）。
		T(((Age as JsonPropertyInfo)?.Get) is not null && ((Age as JsonPropertyInfo)?.Set) is not null, "Age 應同時給出官方 Get 與 Set 委託");
		T(((MemberInfo)Age!).MemberType == MemberTypes.Property, "Age 的官方成員種類應是 Property");
		T(((Age as JsonPropertyInfo)?.AttributeProvider) is not null, "Age 應給出官方特性提供者");

		var Id = Info.Members[0];
		T(Id.DeclaringType == typeof(PoUserBase), "Id 聲明型別應是基類 PoUserBase（繼承成員在前）");

		var Secret = Info.Members[7];
		T(Secret.Name == "Secret", "Secret 應在第 8 位");
		T(Secret.PropertyType == typeof(str), "Secret 型別應是 string");
		T(Secret.CanRead && !Secret.CanWrite, "Secret 應只讀不可寫");
		T(((Secret as JsonPropertyInfo)?.Set) is null, "只讀成員的官方 Set 委託應為 null");

		var Token = Info.Members[9];
		T(Token.Name == "Token", "Token 應在第 10 位");
		T(Token.CanWrite && !Token.CanRead, "Token 應只寫不可讀");
		T(((Token as JsonPropertyInfo)?.Get) is null, "只寫成員的官方 Get 委託應為 null");

		var Note = Info.Members[10];
		T(Note.Name == "Note", "Note 應在第 11 位");
		T(Note.CanRead && Note.CanWrite, "Note 應可讀可寫");

		// 官方出口：反射源給 MemberInfo、Json 源給 JsonPropertyInfo，兩者互斥。
		var First = Info.Members[0];
		if(IsRefl){
			T((First as MemberInfo) is PropertyInfo, $"反射源的官方成員應是 PropertyInfo，實際 {(First as MemberInfo)?.GetType().Name ?? "null"}");
			T((First as JsonPropertyInfo) is null, "反射源不應有 JsonPropertyInfo");
			// [JsonInclude] 字段在反射源是 Field。
			T(((MemberInfo)Info.Members[10]!).MemberType == MemberTypes.Field, "反射源應把 Note 報成 Field");
		}
		else{
			T((First as JsonPropertyInfo) is not null, "Json 源應給出官方 JsonPropertyInfo");
			T((First as MemberInfo) is null, "Json 源沒有反射 MemberInfo（JsonPropertyInfo 不是它的子類）");
			// 官方 JsonPropertyInfo 不暴露 IsProperty，故只能報 Property。
			T(((MemberInfo)Info.Members[10]!).MemberType == MemberTypes.Property, "Json 源只能把 Note 報成 Property");
		}

		// 靜態成員 / 私有字段 / 索引器都不得進成員表。
		foreach(var M in Info.Members){
			T(M.Name is not ("StaticNote" or "Hidden" or "Item"), $"成員 {M.Name} 不應出現（靜態/私有/索引器被排除）");
		}
	}

	/// 見聲明處的說明。
	public partial void RegisterMembers(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "成員表:"
		);
		var R = reg.Register;
		foreach(var Src in _srcs){
			R($"{Src.GetType().Name} 成員表契約", async _ => {
				CheckMembers(InfoOf(Src), Src is ReflTypeInfoSrc);
				return null;
			});
		}
	}
}










