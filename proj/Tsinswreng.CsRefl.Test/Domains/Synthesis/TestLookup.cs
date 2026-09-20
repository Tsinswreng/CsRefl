using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Synthesis;

/// 用法四：按名讀寫的邊界——只讀、只寫、未知名字分別會怎樣。
/// 只放函數實現：聲明在 _TestSynthesis.cs。
public partial class TestSynthesis{
	/// 見聲明處的說明。
	public partial async Task<nil> ReadWriteEdges(obj? O){
		var T = Assert.IsTrue;

		// 起點：門面。
		var Src = new MergedTypeInfoSrc(
			new JsonTypeInfoSrc(TestJsonCtx.Default),
			new ReflTypeInfoSrc()
		);

		var U = new PoUser{Id = 1, Name = "小明", Age = 26};

		// 能取就取：TryGet 不拋，名字是外部來的也不怕。
		T(Src.TryGet(typeof(PoUser), nameof(PoUser.Age), U, out var Age), "Age 取得到");
		T((i32)Age! == 26, $"Age 應是 26，實際 {Age}");
		T(!Src.TryGet(typeof(PoUser), "NoSuch", U, out _), "型別上沒有的名字返回 false");
		T(!Src.TryGet(typeof(PoUser), nameof(PoUser.Token), U, out _), "只寫成員讀不到");

		// 寫：可寫的寫得進；只讀的返回 false 而不拋，也不動物件。
		T(Src.TrySet(typeof(PoUser), nameof(PoUser.Age), U, 31), "Age 寫得進");
		T(U.Age == 31, "寫完物件應變成 31");
		T(!Src.TrySet(typeof(PoUser), nameof(PoUser.Secret), U, "x"), "只讀成員寫不進（返回 false）");
		T(U.Secret == "s", "只讀成員不該被改動");

		// 已經有型別元資料在手時，不必再從來源查一次型別。
		var Info = Src.GetInfo(typeof(PoUser));
		T(Info.TryGet(nameof(PoUser.Name), U, out var Name) && (str)Name! == "小明", "型別元資料自己也能按名讀");

		// 要知道成員的細節：取成員本體（就是官方成員物件），用 MemberExtn 問。
		var M = Info.GetMember(nameof(PoUser.Level));
		T(MemberExtn.DeclaringType(M) == typeof(PoUser), "Level 應宣告在 PoUser 上");
		T(MemberExtn.CanRead(M) && MemberExtn.CanWrite(M), "Level 應可讀可寫");

		return NIL;
	}

	/// 見聲明處的說明。
	public partial void RegisterLookup(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestSynthesis),
			[typeof(ITypeInfoSrcExtn), typeof(ITypeInfo), typeof(MemberExtn)],
			[
				nameof(ITypeInfoSrcExtn.TryGet),
				nameof(ITypeInfoSrcExtn.TrySet),
				nameof(ITypeInfo.GetMember),
			],
			"綜合測試:按名讀寫:"
		);
		reg.Register(nameof(ReadWriteEdges), ReadWriteEdges!);
	}
}
