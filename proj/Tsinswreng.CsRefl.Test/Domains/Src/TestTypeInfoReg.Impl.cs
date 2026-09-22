using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.Src;

/// Reg 的增刪與列舉。用反射元資料當「手寫元資料」的替身（註冊表的內容來源不限）。
/// 只放函數實現：聲明在 TestTypeInfoReg.cs。
public partial class TestTypeInfoReg{
	/// 見聲明處的說明。
	public partial ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = true;
		RegisterAddRemoveEnum(Node);
		return Node;
	}

	/// 見聲明處的說明。
	public partial void RegisterAddRemoveEnum(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfoReg), [typeof(TypeInfoReg)], [nameof(TypeInfoReg.Add)], "註冊表:"
		);
		var R = reg.Register;

		R("Add 後可查且同一實例", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			var Info = new ReflTypeInfo(typeof(PoUser));
			Table.Add(typeof(PoUser), Info);
			T(Table.TryGetInfo(typeof(PoUser), out var Got), "Add 後 TryGetInfo 應命中");
			T(ReferenceEquals(Got, Info), "取回的應是塞進去的那個實例");
			return null;
		});

		R("重複 Add 拋異常", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			Table.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			var Threw = false;
			try{
				Table.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			}
			catch(InvalidOperationException E){
				Threw = true;
				T(E.Message.Contains("已註冊"), $"訊息應說明已註冊，實際：{E.Message}");
			}
			T(Threw, "重複 Add 應拋 InvalidOperationException");
			return null;
		});

		R("Remove 與未註冊查詢", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			Table.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			T(Table.Remove(typeof(PoUser)), "Remove 命中應返回 true");
			T(!Table.Remove(typeof(PoUser)), "再 Remove 應返回 false");
			// 注意：lambda 參數名 _ 會遮蔽 out _ 棄元，這裡用顯式變量。
			var PoUserMiss = Table.TryGetInfo(typeof(PoUser), out var InfoP);
			T(!PoUserMiss, "移除後應查不到");
			T(InfoP is null, "未命中時 out 應為 null");
			var PoColorMiss = Table.TryGetInfo(typeof(PoColor), out var InfoC);
			T(!PoColorMiss, "未註冊型別應查不到");
			T(InfoC is null, "未命中時 out 應為 null");
			return null;
		});

		R("RegisteredTypes 交出註冊表本體", async _ => {
			var T = Assert.IsTrue;
			var Table = new TypeInfoReg();
			Table.Add(typeof(PoUser), new ReflTypeInfo(typeof(PoUser)));
			Table.Add(typeof(PoColor), new ReflTypeInfo(typeof(PoColor)));
			var Types = Table.RegisteredTypes!;
			T(Types.Count == 2, $"列舉應有 2 個，實際 {Types.Count}");
			T(Types.ContainsKey(typeof(PoUser)) && Types.ContainsKey(typeof(PoColor)), "應含剛註冊的兩個型別");
			// 交出的就是註冊表本體（O(1)、不複製），故之後的註冊在同一張表上看得見。
			Table.Add(typeof(PoNoCtor), new ReflTypeInfo(typeof(PoNoCtor)));
			T(Types.Count == 3, "取出的就是同一張表，之後的註冊應看得到");
			return null;
		});
	}
}
