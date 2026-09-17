using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// Attrs 契約：特性查詢是反射獨有能力。
/// - 反射來源：Level 能查到 MyDemoAttr（Tag=優等級、Rank=2），Attrs 含 1 個；
/// - Json來源：Attrs 為空表、TryGetAttr 恒 false（源生成下 AttributeProvider
///   走反射路徑，AOT 不可靠，按已定設計統一不支持）。
public partial class TestTypeInfo{
	/// 反射來源驗證。
	private static void CheckReflAttrs(ITypeInfo Info){
		var T = Assert.IsTrue;
		var Level = Info.GetMember("Level");

		T(Level.Attrs.Count == 1, $"反射下 Level 應有 1 個特性，實際 {Level.Attrs.Count}");
		T(Level.TryGetAttr<MyDemoAttr>(out var Attr), "反射下應能按型別查到 MyDemoAttr");
		T(Attr!.Tag == "優等級", $"特性的 Tag 應是 優等級，實際 {Attr.Tag}");
		T(Attr.Rank == 2, $"特性的 Rank 應是 2，實際 {Attr.Rank}");

		T(!Level.TryGetAttr<ObsoleteAttribute>(out _), "不存在特性型別應返回 false");
		T(!Info.GetMember("Age").TryGetAttr<MyDemoAttr>(out _), "沒標特性的成員應返回 false");
	}

	/// Json來源驗證。
	private static void CheckJsonAttrs(ITypeInfo Info){
		var T = Assert.IsTrue;
		var Level = Info.GetMember("Level");
		T(Level.Attrs.Count == 0, $"Json下 Level 的 Attrs 應為空，實際 {Level.Attrs.Count}");
		T(!Level.TryGetAttr<MyDemoAttr>(out _), "Json下 TryGetAttr 應恒 false");
	}

	public void RegisterAttrs(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Level)], "特性:"
		);
		var R = reg.Register;
		R("反射來源 特性可查", async _ => {
			CheckReflAttrs(InfoOf(_refl));
			return null;
		});
		R("Json來源 特性為空", async _ => {
			CheckJsonAttrs(InfoOf(_json));
			return null;
		});
	}
}