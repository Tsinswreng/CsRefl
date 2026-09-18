using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// MkInst/Kind 契約：分類規則（官方 JsonTypeInfoKind）與實例工廠
/// （官方 JsonTypeInfo.CreateObject 的形狀）。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	private static partial void CheckMkInst(ITypeInfo Info){
		var T = Assert.IsTrue;

		T(Info.Kind == JsonTypeInfoKind.Object, "PoUser 應分類為 Object");
		T(Info.CreateObject is not null, "PoUser 有默認構造函數，應給出官方形狀的無參工廠");
		// CanMkInst 是自研便利，判據就是官方那條「CreateObject 是否為 null」。
		T(Info.CanMkInst, "CanMkInst 應與 CreateObject 非 null 一致");
		var Instance = Info.CreateObject!();
		T(Instance is PoUser, "CreateObject 應產生 PoUser 實例");
		var User = (PoUser)Instance;
		User.Id = 3;
		T(User.Id == 3, "新實例應可直接使用");
		// MkInst 是 CreateObject 的便利包裝。
		T(Info.MkInst() is PoUser, "MkInst 應與 CreateObject 同效");
	}

	/// 見聲明處的說明。
	private static partial void CheckKind(
		ITypeInfo Info, Type T2, JsonTypeInfoKind ExpectKind, Type? ExpectKey, Type? ExpectElem
	){
		var T = Assert.IsTrue;
		T(Info.Kind == ExpectKind, $"{T2.Name} 應分類為 {ExpectKind}，實際 {Info.Kind}");
		T(Info.KeyType == ExpectKey, $"{T2.Name} 的鍵型別應是 {ExpectKey?.Name ?? "null"}，實際 {Info.KeyType?.Name ?? "null"}");
		T(Info.ElementType == ExpectElem, $"{T2.Name} 的元素型別應是 {ExpectElem?.Name ?? "null"}，實際 {Info.ElementType?.Name ?? "null"}");
	}

	/// 見聲明處的說明。
	public partial void RegisterMkInstKind(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Age)], "實例/分類:"
		);
		var R = reg.Register;

		R("反射來源 物件實例", async _ => {
			CheckMkInst(InfoOf(_refl));
			return null;
		});
		R("Json來源 物件實例", async _ => {
			CheckMkInst(InfoOf(_json));
			return null;
		});

		// 標量：官方 JsonTypeInfoKind 對標量給出的就是 None。
		R("反射來源 枚舉=標量(None)", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(PoColor), out var Info), "反射應能查枚舉");
			CheckKind(Info!, typeof(PoColor), JsonTypeInfoKind.None, null, null);
			return null;
		});
		R("Json來源 枚舉=標量(None)", async _ => {
			Assert.IsTrue(_json.TryGetInfo(typeof(PoColor), out var Info), "Json 應能查已註冊枚舉");
			CheckKind(Info!, typeof(PoColor), JsonTypeInfoKind.None, null, null);
			return null;
		});
		R("反射來源 集合與字典", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(List<str>), out var L), "反射應能查 List");
			CheckKind(L!, typeof(List<str>), JsonTypeInfoKind.Enumerable, null, typeof(str));
			Assert.IsTrue(_refl.TryGetInfo(typeof(Dictionary<str, i32>), out var D), "反射應能查 Dictionary");
			CheckKind(D!, typeof(Dictionary<str, i32>), JsonTypeInfoKind.Dictionary, typeof(str), typeof(i32));
			return null;
		});
		R("Json來源 集合與字典", async _ => {
			Assert.IsTrue(_json.TryGetInfo(typeof(List<str>), out var L), "Json 應能查已註冊 List");
			CheckKind(L!, typeof(List<str>), JsonTypeInfoKind.Enumerable, null, typeof(str));
			Assert.IsTrue(_json.TryGetInfo(typeof(Dictionary<str, i32>), out var D), "Json 應能查已註冊 Dictionary");
			CheckKind(D!, typeof(Dictionary<str, i32>), JsonTypeInfoKind.Dictionary, typeof(str), typeof(i32));
			return null;
		});

		R("反射來源 無無參構造=不可建", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(PoNoCtor), out var Info), "反射應能查 PoNoCtor");
			Assert.IsTrue(Info!.Kind == JsonTypeInfoKind.Object, "PoNoCtor 應是 Object");
			Assert.IsTrue(Info.CreateObject is null, "PoNoCtor 沒有無參構造函數，官方形狀的工廠應為 null");
			Assert.IsTrue(!Info.CanMkInst, "CanMkInst 應與 CreateObject 為 null 一致");
			var Threw = false;
			try{
				Info.MkInst();
			}
			catch(NotSupportedException){
				Threw = true;
			}
			Assert.IsTrue(Threw, "對不可建模型別 MkInst 應拋 NotSupportedException");
			return null;
		});

		R("Json來源 未註冊型別查不到", async _ => {
			// 注意：lambda 參數名 _ 會遮蔽 out _ 棄元，這裡用顯式變量。
			var JsonMiss = _json.TryGetInfo(typeof(PoNoCtor), out var JsonInfo);
			Assert.IsTrue(!JsonMiss, "PoNoCtor 未註冊，Json 來源應返回 false");
			Assert.IsTrue(JsonInfo is null, "未命中時 out 應為 null");
			return null;
		});

		R("官方 JsonTypeInfo 出口", async _ => {
			var T = Assert.IsTrue;
			Assert.IsTrue(_json.TryGetInfo(typeof(PoUser), out var JsonInfo), "Json 應能查 PoUser");
			T(JsonInfo!.Json is not null, "Json 源應給出官方 JsonTypeInfo 本體");
			T(JsonInfo.Json!.Type == typeof(PoUser), "官方本體的 Type 應相符");
			// 官方本體就是成員表的來源，兩者口徑必須一致。
			T(JsonInfo.Json.Properties.Count == JsonInfo.Members.Count,
				"官方 Properties 數應與 Members 數一致");
			// 反射源沒有官方 JsonTypeInfo。
			Assert.IsTrue(_refl.TryGetInfo(typeof(PoUser), out var ReflInfo), "反射應能查 PoUser");
			T(ReflInfo!.Json is null, "反射源不應有官方 JsonTypeInfo");
			return null;
		});
	}
}