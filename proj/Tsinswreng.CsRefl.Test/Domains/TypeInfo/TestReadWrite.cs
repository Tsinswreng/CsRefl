using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// ReadWrite 契約：ReadableNames/WritableNames 清單與順序。
public partial class TestTypeInfo{
	/// 對一個來源驗證名字清單。
	private static void CheckReadWriteNames(ITypeInfo Info){
		var T = Assert.IsTrue;

		var Readable = Info.ReadableNames.ToList();
		T(Readable.Count == 10, $"可讀名應有 10 個，實際 {Readable.Count}");
		T(Readable[0] == "Id" && Readable[1] == "Name" && Readable[7] == "Secret" && Readable[9] == "Note",
			"可讀名順序應與成員序一致");

		var Writable = Info.WritableNames.ToList();
		T(Writable.Count == 9, $"可寫名應有 9 個（Secret 只讀），實際 {Writable.Count}");
		T(!Writable.Contains("Secret"), "可寫名不得含 Secret");
		T(Writable[0] == "Id" && Writable[8] == "Note", "可寫名順序應與成員序一致（跳過只讀成員）");
	}

	/// 對一個來源驗證 TryGet/TrySet 端到端（造一個實例走完整讀寫）。
	private static void CheckReadWriteE2E(ITypeInfo Info){
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

		// 實例型別不符 / null 實例。
		T(!Info.GetMember("Age").TryGet(null, out _), "null 實例讀應返回 false");
		T(!Info.GetMember("Age").TryGet(new PoColor(), out _), "錯誤型別實例讀應返回 false");
	}

	public void RegisterReadWrite(ITestNode Node){
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