using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 層三：成員層 IMemberInfo——成員物件是官方物件（反射側 MemberInfo、Json 側 JsonPropertyInfo）的配接器。
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
		T(MAge.Name == nameof(PoUser.Age), "成員名應是 Age");
		T(MAge.DeclaringType == typeof(PoUser), "Age 應宣告在 PoUser 上");
		T(Info.GetMember(nameof(PoUser.Id)).DeclaringType == typeof(PoUserBase), "繼承成員 Id 的宣告型別是基類");

		// 宣告型別：PropertyInfo／JsonPropertyInfo 取 PropertyType、FieldInfo 取 FieldType。
		T(MAge.PropertyType == typeof(i32), "Age 的宣告型別應是 i32");
		T(Info.GetMember(nameof(PoUser.Note)).PropertyType == typeof(str), "Note 是字段，宣告型別應是 str");
		T(Info.GetMember(nameof(PoUser.Tags)).PropertyType == typeof(List<str>), "Tags 的宣告型別應是 List<str>");
		T(true, "（成員契約 IMemberInfo 沒有『認不得的物件』這條分叉，原斷言已移除）");

		// 可讀可寫：判據是成員自身能力，不是「在不在某張鍵表裏」。
		T(MAge.CanRead && MAge.CanWrite, "Age 可讀可寫");
		T(Info.GetMember(nameof(PoUser.Secret)).CanRead && !Info.GetMember(nameof(PoUser.Secret)).CanWrite
			, "Secret 只讀");
		T(!Info.GetMember(nameof(PoUser.Token)).CanRead && Info.GetMember(nameof(PoUser.Token)).CanWrite
			, "Token 只寫");

		// 讀寫：成員在前、實例在後（成員物件自己知道怎麼讀寫自己）。
		T(MAge.TryGet(U, out var V) && (i32)V! == 26, "讀 Age 應得到 26");
		T(MAge.TrySet(U, 33) && U.Age == 33, "寫 Age 應寫回物件");
		T(!MAge.TryGet(null, out _), "實例為 null 返回 false");
		T(!MAge.TryGet(new PoColor(), out _), "實例與宣告型別不符返回 false");
		T(!Info.GetMember(nameof(PoUser.Secret)).TrySet(U, "x"), "只讀成員寫不進");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterMember(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(IMemberInfo)],
			[nameof(IMemberInfo.PropertyType), nameof(IMemberInfo.TryGet)],
			"綜合測試:成員層:"
		);
		reg.Register(nameof(MemberOps), MemberOps!);
	}
}









