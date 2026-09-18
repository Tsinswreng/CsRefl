using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// ReadWrite 契約：ReadableNames/WritableNames 清單與順序、TryGet/TrySet 端到端。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	private static partial void CheckReadWriteNames(ITypeInfo Info){
		var T = Assert.IsTrue;

		var Readable = Info.ReadableNames.ToList();
		T(Readable.Count == 10, $"可讀名應有 10 個（Token 只寫），實際 {Readable.Count}");
		T(Readable[0] == "Id" && Readable[1] == "Name" && Readable[7] == "Secret" && Readable[9] == "Note",
			"可讀名順序應與成員序一致");

		var Writable = Info.WritableNames.ToList();
		T(Writable.Count == 10, $"可寫名應有 10 個（Secret 只讀），實際 {Writable.Count}");
		T(!Writable.Contains("Secret"), "可寫名不得含 Secret");
		T(Writable[0] == "Id" && Writable[8] == "Token" && Writable[9] == "Note",
			"可寫名順序應與成員序一致（跳過只讀成員、含只寫成員）");
	}

	/// 見聲明處的說明。
	private static partial void CheckReadWriteE2E(ITypeInfo Info){
		var T = Assert.IsTrue;
		var User = new PoUser{ Id = 7, Name = "小明", Age = 30 };

		// 讀：Age=30、Email=null（可空未賦值）。
		T(Info.GetMember("Age").TryGet(User, out var AgeR), "讀 Age 應成功");
		T((i32)AgeR! == 30, "Age 讀值應為 30");
		T(Info.GetMember("Email").TryGet(User, out var EmailR), "讀 Email 應成功");
		T(EmailR is null, "Email 未賦值應讀到 null");
		T(Info.GetMember("Secret").TryGet(User, out var SecretR), "讀 Secret 應成功");
		T((str)SecretR! == "s", "Secret 讀值應為初始值 s");

		// 寫：Age、Name（繼承成員也寫得動）。
		T(Info.GetMember("Age").TrySet(User, 31), "寫 Age 應成功");
		T(Info.GetMember("Age").TryGet(User, out var AgeR2) && (i32)AgeR2! == 31, "寫後 Age 應為 31");
		T(Info.GetMember("Name").TrySet(User, "阿強"), "寫繼承成員 Name 應成功");
		T(User.Name == "阿強", "Name 應已寫回物件");

		// 只讀成員寫不動。
		T(!Info.GetMember("Secret").TrySet(User, "x"), "寫只讀 Secret 應返回 false");

		// 只寫成員讀不到（它照樣在成員表與 WritableNames 裏）。
		T(!Info.GetMember("Token").TryGet(User, out _), "讀只寫 Token 應返回 false");

		// 實例型別不符 / null 實例。
		T(!Info.GetMember("Age").TryGet(null, out _), "null 實例讀應返回 false");
		T(!Info.GetMember("Age").TryGet(new PoColor(), out _), "錯誤型別實例讀應返回 false");
	}

	/// 見聲明處的說明。
	public partial void RegisterReadWrite(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "讀寫:"
		);
		var R = reg.Register;
		R("反射來源 名字清單契約", async _ => {
			CheckReadWriteNames(InfoOf(_refl));
			return null;
		});
		R("Json來源 名字清單契約", async _ => {
			CheckReadWriteNames(InfoOf(_json));
			return null;
		});
		R("反射來源 讀寫端到端", async _ => {
			CheckReadWriteE2E(InfoOf(_refl));
			return null;
		});
		R("Json來源 讀寫端到端", async _ => {
			CheckReadWriteE2E(InfoOf(_json));
			return null;
		});
	}
}