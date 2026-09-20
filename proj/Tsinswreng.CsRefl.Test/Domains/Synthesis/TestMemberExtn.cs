using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 層三：成員層 Member——成員本體就是官方物件（反射側 MemberInfo、Json 側 JsonPropertyInfo）。
/// 這一層沒有泛型版：受體就是成員本體本身，沒有「型別要從哪來」的問題。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> MemberOps(obj? O){
		var T = Assert.IsTrue;

		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);
		var U = new PoUser{Id = 1, Name = "小明", Age = 26};
		var Info = Src.GetInfo(typeof(PoUser));

		var MAge = Info.GetMember(nameof(PoUser.Age));

		// 名字、宣告型別：兩種成員物件（屬性／字段）與兩套來源同一條路。
		T(Member.Name(MAge) == nameof(PoUser.Age), "成員名應是 Age");
		T(Member.DeclaringType(MAge) == typeof(PoUser), "Age 應宣告在 PoUser 上");
		T(Member.DeclaringType(Info.GetMember(nameof(PoUser.Id))) == typeof(PoUserBase), "繼承成員 Id 的宣告型別是基類");

		// 宣告型別：PropertyInfo／JsonPropertyInfo 取 PropertyType、FieldInfo 取 FieldType。
		T(Member.PropertyType(MAge) == typeof(i32), "Age 的宣告型別應是 i32");
		T(Member.PropertyType(Info.GetMember(nameof(PoUser.Note))) == typeof(str), "Note 是字段，宣告型別應是 str");
		T(Member.PropertyType(Info.GetMember(nameof(PoUser.Tags))) == typeof(List<str>), "Tags 的宣告型別應是 List<str>");
		T(Member.PropertyType("不是成員") is null, "認不得的物件返回 null");

		// 可讀可寫：判據是成員自身能力，不是「在不在某張鍵表裏」。
		T(Member.CanRead(MAge) && Member.CanWrite(MAge), "Age 可讀可寫");
		T(Member.CanRead(Info.GetMember(nameof(PoUser.Secret))) && !Member.CanWrite(Info.GetMember(nameof(PoUser.Secret)))
			, "Secret 只讀");
		T(!Member.CanRead(Info.GetMember(nameof(PoUser.Token))) && Member.CanWrite(Info.GetMember(nameof(PoUser.Token)))
			, "Token 只寫");

		// 讀寫：成員本體在前、實例在後（擴展方法的受體就是成員）。
		T(Member.TryGet(MAge, U, out var V) && (i32)V! == 26, "讀 Age 應得到 26");
		T(Member.TrySet(MAge, U, 33) && U.Age == 33, "寫 Age 應寫回物件");
		T(!Member.TryGet(MAge, null, out _), "實例為 null 返回 false");
		T(!Member.TryGet(MAge, new PoColor(), out _), "實例與宣告型別不符返回 false");
		T(!Member.TrySet(Info.GetMember(nameof(PoUser.Secret)), U, "x"), "只讀成員寫不進");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterMember(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(Member)],
			[nameof(Member.PropertyType), nameof(Member.TryGet)],
			"綜合測試:成員層:"
		);
		reg.Register(nameof(MemberOps), MemberOps!);
	}
}


