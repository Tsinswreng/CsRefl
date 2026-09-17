using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// MkInst/Kind 契約：分類規則與無參實例工廠。
public partial class TestTypeInfo{
	/// 對一個來源驗證 PoUser 的實例工廠。
	private static void CheckMkInst(ITypeInfo Info){
		var T = Assert.IsTrue;

		T(Info.Kind == ETypeKind.Object, "PoUser 應分類為 Object");
		T(Info.CanMkInst, "PoUser 有默認構造函數，應可建實例");
		var Instance = Info.MkInst();
		T(Instance is PoUser, "MkInst 應產生 PoUser 實例");
		var User = (PoUser)Instance!;
		User.Id = 3;
		T(User.Id == 3, "新實例應可直接使用");
	}

	/// 對一個來源驗證分類與鍵值型別。
	private static void CheckKind(ITypeInfo Info, Type T2, ETypeKind ExpectKind, Type? ExpectKey, Type? ExpectElem){
		var T = Assert.IsTrue;
		T(Info.Kind == ExpectKind, $"{T2.Name} 應分類為 {ExpectKind}，實際 {Info.Kind}");
		T(Info.KeyType == ExpectKey, $"{T2.Name} 的鍵型別應是 {ExpectKey?.Name ?? "null"}，實際 {Info.KeyType?.Name ?? "null"}");
		T(Info.ElemType == ExpectElem, $"{T2.Name} 的元素型別應是 {ExpectElem?.Name ?? "null"}，實際 {Info.ElemType?.Name ?? "null"}");
	}

	public void RegisterMkInstKind(ITestNode Node){
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

		R("反射來源 枚舉=標量", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(PoColor), out var Info), "反射應能查枚舉");
			CheckKind(Info!, typeof(PoColor), ETypeKind.Scalar, null, null);
			return null;
		});
		R("Json來源 枚舉=標量", async _ => {
			Assert.IsTrue(_json.TryGetInfo(typeof(PoColor), out var Info), "Json 應能查已註冊枚舉");
			CheckKind(Info!, typeof(PoColor), ETypeKind.Scalar, null, null);
			return null;
		});
		R("反射來源 集合與字典", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(List<str>), out var L), "反射應能查 List");
			CheckKind(L!, typeof(List<str>), ETypeKind.Enumerable, null, typeof(str));
			Assert.IsTrue(_refl.TryGetInfo(typeof(Dictionary<str, i32>), out var D), "反射應能查 Dictionary");
			CheckKind(D!, typeof(Dictionary<str, i32>), ETypeKind.Dictionary, typeof(str), typeof(i32));
			return null;
		});
		R("Json來源 集合與字典", async _ => {
			Assert.IsTrue(_json.TryGetInfo(typeof(List<str>), out var L), "Json 應能查已註冊 List");
			CheckKind(L!, typeof(List<str>), ETypeKind.Enumerable, null, typeof(str));
			Assert.IsTrue(_json.TryGetInfo(typeof(Dictionary<str, i32>), out var D), "Json 應能查已註冊 Dictionary");
			CheckKind(D!, typeof(Dictionary<str, i32>), ETypeKind.Dictionary, typeof(str), typeof(i32));
			return null;
		});

		R("反射來源 無無參構造=不可建", async _ => {
			Assert.IsTrue(_refl.TryGetInfo(typeof(PoNoCtor), out var Info), "反射應能查 PoNoCtor");
			Assert.IsTrue(Info!.Kind == ETypeKind.Object, "PoNoCtor 應是 Object");
			Assert.IsTrue(!Info.CanMkInst, "PoNoCtor 沒有無參構造函數，CanMkInst 應為 false");
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
	}
}