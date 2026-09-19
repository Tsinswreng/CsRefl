namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源的型別元資料：對一個 {nameof(Type)} 建立 {nameof(ITypeInfo)}。]

#Descr[
分類靠介面分析（`{nameof(IDictionary<,>)}` 是字典、`{nameof(IEnumerable<>)}` 是集合、
基本型別是標量、其餘是物件），
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。

例：`{nameof(List<int>)}` 判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}、
`{nameof(Dictionary<string, int>)}` 判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}、
`i32` 判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}、
有成員的類判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}。
]

#Descr[
成員只收公開實例屬性與公開實例字段，順序 = 契約序
（{nameof(TypeInfoSorter)}：基類在前、同類內屬性段在字段段前、段內收集序；
同名遮蔽只留最靠近實例的那份）。

注意屬性與字段跨 table 沒有統一的源碼行號，
源碼裏屬性字段交錯聲明時，同類內一律「屬性在前、字段在後」。

例：一個類先聲明字段 `A`、再聲明屬性 `B`，
本來源給出的順序仍是 `B` 在前、`A` 在後。
這不是看漏了源碼順序，而是刻意選定的穩定序：
{nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)}
是兩次收集，跨 table 沒有共同序可依。
]

#Descr[
建構子與 {nameof(MkInst)} 實現見 `ReflTypeInfo.Impl.cs`。
]
""")]
public partial class ReflTypeInfo:TypeInfoBase{
	[Doc($"""
#Sum[無參實例工廠；null 表示本型別不可建實例。]

#Descr[
型別與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致，
對外以 {nameof(CreateObject)} 暴露。

例：有公開無參構造函數的類，這個字段在 JIT 下是表達式樹編譯出來的委託；
NativeAOT 下表達式樹不能 Compile（會拋 {nameof(PlatformNotSupportedException)}），
故退成每次調 {nameof(Activator)}.{nameof(Activator.CreateInstance)}。
]
""")]
	private readonly Func<obj>? _mkInstFn;

	[Doc($"""
#Sum[對一個型別建立元資料。]

#Params([[要建立元資料的型別]])

#Descr[
DAM 註解：
反射建立元資料需要 接口、公共屬性、公共字段、無參構造函數 的元數據被保留
（AOT 剪裁的前提，見 {nameof(ReflMemberInfo)} 的說明）。

例：`new {nameof(ReflTypeInfo)}(typeof(User))` 一次做完分類、收集成員、找鍵值型別、建無參工廠；
之後 {nameof(Members)} 與 {nameof(GetMember)} 都直接用這份結果，不會每次重算。
]
""")]
	public partial ReflTypeInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type
	);

	[Doc($"""
#Sum[無參實例工廠，形狀與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致。]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return _mkInstFn;
		}
	}

	[Doc($"""
#Sum[反射來源沒有官方 {nameof(JsonTypeInfo)}，恆為 null。]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public override JsonTypeInfo? Json{
		get{
			return null;
		}
	}

	[Doc($"""
#Sum[建立無參實例；不可建時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();
}