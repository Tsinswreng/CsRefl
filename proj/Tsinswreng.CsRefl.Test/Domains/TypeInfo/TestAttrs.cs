using System.Reflection;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;

namespace Tsinswreng.CsRefl.Test.Domains.TypeInfo;

/// 特性契約：走官方那一條路——IMemberInfo.AttributeProvider（官方
/// ICustomAttributeProvider）＋官方取特性的語義（不繼承、無則 null）。
///
/// 為甚麼用門面的 GetCustomAttribute 擴展而不是官方那個同名擴展：
/// 官方只把它掛在 MemberInfo 等具體型別上，而兩套來源共有的官方接口是
/// ICustomAttributeProvider，它上面沒有這個擴展（已用編譯器核實）。
/// 本包的擴展只是同一語義的補位，實現就是轉官方 GetCustomAttributes。
///
/// 重要修正：早期代碼斷言「源生成下 AttributeProvider 走反射路徑、AOT 不可靠，
/// 故 Json 源的特性統一為空」。實測推翻：源生成給出的 AttributeProvider 是
/// RuntimePropertyInfo／RtFieldInfo，Level 上的 MyDemoAttr 真的取得到，
/// 且 AOT 跑同一份斷言照樣通過（見 TestAotWin）。故兩套來源都按官方取特性。
/// 函數實現文件；聲明在 _TestTypeInfo.cs。
public partial class TestTypeInfo{
	/// 見聲明處的說明。
	private static partial void CheckAttrs(ITypeInfo Info, bool IsRefl){
		var T = Assert.IsTrue;
		var Level = Info.GetMember("Level");

		T(Level.AttributeProvider is not null, "每個成員都應給出官方特性提供者");

		var Attr = Level.GetCustomAttribute<MyDemoAttr>();
		T(Attr is not null, $"{(IsRefl ? "反射" : "Json")}源應能取到 Level 上的 MyDemoAttr");
		T(Attr!.Tag == "優等級", $"特性的 Tag 應是 優等級，實際 {Attr.Tag}");
		T(Attr.Rank == 2, $"特性的 Rank 應是 2，實際 {Attr.Rank}");

		// 官方提供者的用法也照官方三態：判有無、取全部、取不存在。
		T(Level.AttributeProvider!.IsDefined(typeof(MyDemoAttr), false), "IsDefined 應為 true");
		T(Level.AttributeProvider.GetCustomAttributes(typeof(MyDemoAttr), false).Length == 1,
			"應恰好取到 1 個 MyDemoAttr");
		T(Level.GetCustomAttribute<ObsoleteAttribute>() is null, "不存在的特性應取到 null");
		T(Info.GetMember("Age").GetCustomAttribute<MyDemoAttr>() is null,
			"沒標特性的成員應取到 null");

		// 反射源還可從官方成員本體直接拿（同一份提供者）。
		if(IsRefl){
			T(Level.Member is PropertyInfo, "反射源的官方成員應是 PropertyInfo");
			T(ReferenceEquals(Level.AttributeProvider, Level.Member), "反射源的特性提供者就是官方成員自身");
		}
	}

	/// 見聲明處的說明。
	public partial void RegisterAttrs(ITestNode Node){
		var reg = Node.MkTestFnRegister(
			typeof(TestTypeInfo), [typeof(PoUser)], [nameof(PoUser.Level)], "特性:"
		);
		var R = reg.Register;
		foreach(var Src in _srcs){
			R($"{Src.GetType().Name} 特性可查", async _ => {
				CheckAttrs(InfoOf(Src), Src is ReflTypeInfoSrc);
				return null;
			});
		}
	}
}