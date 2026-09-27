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
			var M = Info.Members.Values.ElementAt(I);
			T(M.Name == ExpectOrder[I], $"第 {I} 個成員應是 {ExpectOrder[I]}，實際 {M.Name}");
		}

		var Age = Info.Members.Values.ElementAt(2);
		T(Age.Name == "Age", "Age 應在自分類的第 3 位");
		T(Age.PropertyType == typeof(i32), "Age 型別應是 int");
		T(Age.OwnerType == typeof(PoUser), "Age 聲明型別應是 PoUser");
		T(Age.CanRead && Age.CanWrite, "Age 應可讀可寫");
		// 成員能力與官方出口同一判據：反射側看官方 getter/setter 方法，Json 側看官方 Get/Set 委託。
		// 成員層改成配接器之後，官方物件本體不再經 IMemberInfo 交出（設計如此），故從配接器的 _Raw 取。
		var RawAge = IsRefl ? (obj)((ReflMemberInfo)Age)._Raw : ((JsonMemberInfo)Age)._Raw;
		if(IsRefl){
			var AgeProp = (PropertyInfo)RawAge;
			T(AgeProp.GetGetMethod() is not null && AgeProp.GetSetMethod() is not null, "Age 應同時給出官方 getter 與 setter 方法");
			T(AgeProp.MemberType == MemberTypes.Property, "Age 的官方成員種類應是 Property");
		}
		else{
			var AgeJson = (JsonPropertyInfo)RawAge;
			T(AgeJson.Get is not null && AgeJson.Set is not null, "Age 應同時給出官方 Get 與 Set 委託");
		}
		T(Age.AttributeProvider is not null, "Age 應給出官方特性提供者");

		var Id = Info.Members.Values.ElementAt(0);
		T(Id.OwnerType == typeof(PoUserBase), "Id 聲明型別應是基類 PoUserBase（繼承成員在前）");

		var Secret = Info.Members.Values.ElementAt(7);
		T(Secret.Name == "Secret", "Secret 應在第 8 位");
		T(Secret.PropertyType == typeof(str), "Secret 型別應是 string");
		T(Secret.CanRead && !Secret.CanWrite, "Secret 應只讀不可寫");
		var RawSecret = IsRefl ? (obj)((ReflMemberInfo)Secret)._Raw : ((JsonMemberInfo)Secret)._Raw;
		T(IsRefl
			? ((PropertyInfo)RawSecret).GetSetMethod() is null
			: ((JsonPropertyInfo)RawSecret).Set is null,
			"只讀成員的官方寫入面應不存在（反射側沒有公開 setter、Json 側 Set 委託為 null）");

		var Token = Info.Members.Values.ElementAt(9);
		T(Token.Name == "Token", "Token 應在第 10 位");
		T(Token.CanWrite && !Token.CanRead, "Token 應只寫不可讀");
		var RawToken = IsRefl ? (obj)((ReflMemberInfo)Token)._Raw : ((JsonMemberInfo)Token)._Raw;
		T(IsRefl
			? ((PropertyInfo)RawToken).GetGetMethod() is null
			: ((JsonPropertyInfo)RawToken).Get is null,
			"只寫成員的官方讀取面應不存在（反射側沒有公開 getter、Json 側 Get 委託為 null）");

		var Note = Info.Members.Values.ElementAt(10);
		T(Note.Name == "Note", "Note 應在第 11 位");
		T(Note.CanRead && Note.CanWrite, "Note 應可讀可寫");

		// 官方出口：反射源給 MemberInfo、Json 源給 JsonPropertyInfo；
		// JsonPropertyInfo 不是 MemberInfo 的子類，故 Json 源沒有 MemberType 那條可看。
		if(IsRefl){
			var First = (ReflMemberInfo)Info.Members.Values.ElementAt(0);
			T(First._Raw is PropertyInfo, $"反射源的官方成員應是 PropertyInfo，實際 {First._Raw.GetType().Name}");
			// [JsonInclude] 字段在反射源是 Field。
			T(((ReflMemberInfo)Info.Members.Values.ElementAt(10))._Raw.MemberType == MemberTypes.Field, "反射源應把 Note 報成 Field");
		}
		else{
			var First = (JsonMemberInfo)Info.Members.Values.ElementAt(0);
			T(First._Raw.Name == nameof(PoUser.Id), $"Json 源的官方物件應是 Id 那個 JsonPropertyInfo，實際 {First._Raw.Name}");
			// 官方 JsonPropertyInfo 不暴露成員種類，故 Json 源只能靠 PropertyType 認成員（見下一句）。
			T(((JsonMemberInfo)Info.Members.Values.ElementAt(10))._Raw.PropertyType == typeof(str), "Json 源把 Note 報成 str 成員");
		}

		// 靜態成員 / 私有字段 / 索引器都不得進成員表。
		foreach(var M in Info.Members.Values){
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










