using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 用法五：泛型入口——型別在編譯期就知道時，泛型版比非泛型版少寫一個 typeof。
/// 兩條路並存，不是替代：型別運行期才知道（例如要按 O.GetType() 查）仍走非泛型版。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> GenericEntry(obj? O){
		var T = Assert.IsTrue;

		// 起點：門面。
		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);

		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		// 查型別：GetInfo<T>() 等於 GetInfo(typeof(T))；
		// 差別是 DAM 掛在 T 上，剪裁器看得見「這個型別需要成員元數據」。
		var Info = Src.GetInfo<PoUser>();
		T(Info.Type == typeof(PoUser), "GetInfo<PoUser> 應取到 PoUser 的元資料");

		// 來源層的泛型版：按名讀寫。
		T(Src.TryGet<PoUser>(U, nameof(PoUser.Age), out var Age), "TryGet<PoUser> 應取到 Age");
		T((i32)Age! == 26, $"Age 應是 26，實際 {Age}");
		T(Src.TrySet<PoUser>(U, nameof(PoUser.Age), 27), "TrySet<PoUser> 應寫得進");
		T(U.Age == 27, "寫完物件應變成 27");

		// 來源層的泛型版：取成員。
		var M = Src.GetMember<PoUser>(nameof(PoUser.Age));
		T(MemberExtn.Name(M) == nameof(PoUser.Age), "GetMember<PoUser> 應取到 Age");
		T(Src.TryGetMember<PoUser>(nameof(PoUser.Level), out _), "TryGetMember<PoUser> 應取到 Level");

		// 成員型別：MemberExtn.PropertyType 兩側一條口徑（屬性與字段都在這條路上）。
		T(MemberExtn.PropertyType(M) == typeof(i32), "Age 的宣告型別應是 i32");
		T(
			MemberExtn.PropertyType(Src.GetMember<PoUser>(nameof(PoUser.Note))) == typeof(str),
			"Note 是字段，宣告型別應是 str"
		);

		// 型別元資料層的泛型版（ITypeInfoExtn）。
		T(
			Info.TryGet<PoUser>(U, nameof(PoUser.Name), out var Name) && (str)Name! == "小明",
			"Info.TryGet<PoUser> 應取到 Name"
		);
		T(Info.TrySet<PoUser>(U, nameof(PoUser.Name), "小紅"), "Info.TrySet<PoUser> 應寫得進");
		T(U.Name == "小紅", "寫完物件應變成 小紅");

		// 字典視圖：泛型版按「靜態型別」，非泛型版按「執行期型別」——這條差別要記住。
		T(Src.ToInstDict<PoUser>(U).Count == 9, "ToInstDict<PoUser> 應有 9 個鍵");
		PoUserBase B = U;
		T(Src.ToInstDict<PoUserBase>(B).Count == 2, "ToInstDict<PoUserBase> 按靜態型別應只有 2 個鍵");
		T(Src.ToInstDict(B).Count == 9, "非泛型版 ToInstDict(B) 按執行期型別應是 9 個鍵");

		// 回填：泛型版同樣只認 T 那張成員表。
		Src.AssignFromDict<PoUser>(U, new Dictionary<str, obj?>{
			[nameof(PoUser.Level)] = 7,
		});
		T(U.Level == 7, "AssignFromDict<PoUser> 應把 Level 寫回去");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterGenericEntry(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(ITypeInfoSrcExtn), typeof(ITypeInfoExtn), typeof(MemberExtn)],
			[nameof(ITypeInfoSrcExtn.GetInfo)],
			"綜合測試:泛型入口:"
		);
		reg.Register(nameof(GenericEntry), GenericEntry!);
	}
}
