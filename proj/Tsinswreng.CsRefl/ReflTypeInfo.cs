namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[反射來源的型別元資料：對一個 `Type` 建立 `ITypeInfo`。]

#Descr[
分類靠介面分析（`IDictionary`→字典、`IEnumerable`→集合、基本型別→標量、其餘→物件），
結果映射到官方 `JsonTypeInfoKind`（標量是 `None`）。
]

#Descr[
成員只收公開實例屬性與公開實例字段，順序 = 契約序
（`TypeInfoSorter`：基類在前、同類內屬性段在字段段前、段內收集序；
同名遮蔽只留最靠近實例的那份）。

注意屬性/字段跨 table 沒有統一的源碼行號，
源碼裏屬性字段交錯聲明時，同類內一律「屬性在前、字段在後」。
]

#Descr[
建構子與 `MkInst` 實現見 `ReflTypeInfo.Impl.cs`。
]
""")]
public partial class ReflTypeInfo:TypeInfoBase{
	[Doc("""
#Sum[無參實例工廠；null 表示本型別不可建實例。]

#Descr[
型別與官方 `JsonTypeInfo.CreateObject` 一致，對外以 `CreateObject` 暴露。
]
""")]
	private readonly Func<obj>? _mkInstFn;

	[Doc("""
#Sum[對一個型別建立元資料。]

#Params([[要建立元資料的型別]])

#Descr[
DAM 註解：
反射建立元資料需要 接口/公共屬性/公共字段/無參構造函數 的元數據被保留
（AOT 剪裁的前提，見 `ReflMemberInfo` 的說明）。
]
""")]
	public partial ReflTypeInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type
	);

	[Doc("""
#Sum[無參實例工廠，形狀與官方 `JsonTypeInfo.CreateObject` 一致。]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return _mkInstFn;
		}
	}

	[Doc("""
#Sum[反射來源沒有官方 `JsonTypeInfo`，恆為 null。]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public override JsonTypeInfo? Json{
		get{
			return null;
		}
	}

	[Doc("""
#Sum[建立無參實例；不可建時拋 `NotSupportedException`。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();
}