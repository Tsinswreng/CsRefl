namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源的型別元資料：對一個 {nameof(Type)} 建立 {nameof(ITypeInfo)}。]

#Descr[
分類靠介面分析（`{nameof(IDictionary<,>)}` 是字典、`{nameof(IEnumerable<>)}` 是集合、
基本型別是標量、其餘是物件），
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。

實測（`PoUser`）：{nameof(Kind)} 是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
`typeof(List<str>)` 得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
`typeof(Dictionary<str, i32>)` 得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
`typeof(PoColor)`（枚舉）得到 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}。
]

#Descr[
成員只收公開實例屬性與公開實例字段，順序 = 契約序
（{nameof(TypeInfoSorter)}：基類在前、同類內屬性段在字段段前、段內收集序；
同名遮蔽只留最靠近實例的那份）。

注意屬性與字段跨 table 沒有統一的源碼行號，
源碼裏屬性字段交錯聲明時，同類內一律「屬性在前、字段在後」。

這不是看漏了源碼順序，而是刻意選定的穩定序：
{nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)}
是兩次收集，跨 table 沒有共同序可依。

實測（`PoUser`）：{nameof(Members)} 是 11 項，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`；
其中 `Note`（字段）排在 `Token`（屬性）之後，正是「屬性段先、字段段後」的體現。
]

#Descr[
建構子與 {nameof(MkInst)} 實現見 `ReflTypeInfo.Impl.cs`。
]
""")]
public partial class ReflTypeInfo:TypeInfoBase{
	[Doc($"""
#Sum[反射建立元資料所需的成員種類。]

#Descr[
{nameof(ITypeInfoSrc)}、{nameof(ITypeInfoSrcExtn)} 的 DAM 註解都引用本常量，
改動即全包同步。

實測：`typeof(PoUser)` 的成員表能取到 11 項、無參工廠非 null，
即「公共屬性、公共字段、無參構造函數」這三檔元數據在 NativeAOT 下確實被保留。
]
""")]
	internal const DynamicallyAccessedMemberTypes ReflDam
		= DynamicallyAccessedMemberTypes.Interfaces
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicFields
		| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

	[Doc($"""
#Sum[無參實例工廠；null 表示本型別不可建實例。]
]

#Descr[
型別與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致，
對外以 {nameof(CreateObject)} 暴露。

實測：`typeof(PoUser)` 這個字段非 null，調一次得到一個 `PoUser`；
`typeof(PoNoCtor)`（只有帶參構造函數）為 null。
JIT 下它由表達式樹一次編譯成委託；
NativeAOT 下表達式樹不能 Compile（會拋 {nameof(PlatformNotSupportedException)}），
故退成每次調 {nameof(Activator)}.{nameof(Activator.CreateInstance)}。
]
""")]
	private readonly Func<obj>? _mkInstFn;

	[Doc($"""
#Sum[對一個型別建立元資料。]

#Params([[Type, 要建立元資料的型別]])

#Descr[
DAM 註解：
反射建立元資料需要 接口、公共屬性、公共字段、無參構造函數 的元數據被保留
（AOT 剪裁的前提，見 {nameof(ReflMemberInfo)} 的說明）。

實測：`new {nameof(ReflTypeInfo)}(typeof(PoUser))` 一次做完分類、收集成員、找鍵值型別、建無參工廠；
之後 {nameof(Members)} 與 {nameof(GetMember)} 都直接用這份結果，不會每次重算
（`{nameof(GetMember)}("Age")` 與 `{nameof(TryGetMember)}("Age", out _)` 返回同一實例）。
]
""")]
	public partial ReflTypeInfo(
		Type Type
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

	// ---- 私有輔助（實現見 ReflTypeInfo.Impl.cs）----
	// 參數上的剪裁註解（{nameof(DynamicallyAccessedMembersAttribute)}）只寫在 Impl 側，
	// `partial` 合併時兩邊都標會報 CS0579。

	[Doc($"""
#Sum[型別分類。]

#Params([[T, 要分類的型別]])

#Rtn[官方 {nameof(JsonTypeInfoKind)} 分類結果]

#Descr[
先剝 {nameof(Nullable<int>)}，
再按 字典、標量、集合、物件 的優先序判定，
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量對應官方的 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}，
見 {nameof(ITypeInfo.Kind)} 的說明）。

實測取值：

+ `typeof(PoColor)`（枚舉）→ {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}；
+ `typeof(List<str>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
+ `typeof(Dictionary<str, i32>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
+ `typeof(PoUser)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
+ `typeof(DateTime[])` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}
	（集合優先於元素型別的標量性，故不是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。
]
""")]
	private static partial JsonTypeInfoKind ComputeKind(Type T);

	[Doc($"""
#Sum[是否字典。]

#Params([[T, 要判定的型別]])

#Rtn[是字典返回 true]

#Descr[
實現了非泛型 {nameof(System.Collections.IDictionary)}，
或（直接是或實現了）{nameof(IDictionary<,>)}。

實測：`typeof(Dictionary<str, i32>)` 兩條都命中；
`typeof(List<str>)` 兩條都不命中，返回 false。
]
""")]
	private static partial bool IsDictionary(Type T);

	[Doc($"""
#Sum[找集合的元素型別。]

#Params([[T, 要查找的型別]])

#Rtn[元素型別；非集合為 null]

#Descr[
數組取 {nameof(Type.GetElementType)}；
字典取值型別（與官方 {nameof(JsonTypeInfo.ElementType)} 一致，
官方對字典的元素型別就是值型別）；
其餘取 `{nameof(IEnumerable<>)}` 的泛型實參。

實測取值：

+ `typeof(i32[])` → `typeof(i32)`；
+ `typeof(Dictionary<str, i32>)` → `typeof(i32)`（值型別，不是鍵型別）；
+ `typeof(List<str>)` → `typeof(str)`；
+ `typeof(i32)`、`typeof(PoColor)` → null。
]
""")]
	private static partial Type? FindElementType(Type T);

	[Doc($"""
#Sum[找字典的鍵型別。]

#Params([[T, 要查找的型別]])

#Rtn[鍵型別；非字典為 null]

#Descr[
即 `{nameof(IDictionary<,>)}` 的第一個泛型實參。

實測：`typeof(Dictionary<str, i32>)` → `typeof(str)`；
`typeof(List<str>)` 不是字典，返回 null
（此時 {nameof(TypeInfoBase.ElementType)} 有值 `typeof(str)`、本項為 null）。
]
""")]
	private static partial Type? FindKeyType(Type T);

	[Doc($"""
#Sum[找 `T` 上實現了「泛型定義為 `Def`」的最近介面。]

#Params([[T, 要查找的型別], [Def, 泛型介面的開放泛型定義]])

#Rtn[找到的介面型別；沒有為 null]

#Descr[
`T` 本身是該介面也認。

實測：`Def` 傳 `typeof({nameof(IDictionary<,>)})` 時，
`typeof(Dictionary<str, i32>)` 返回它自己實現的 `{nameof(IDictionary<string, int>)}`；
`typeof(List<str>)` 返回 null。

只認第一個匹配的介面，故有多個同定義介面時取 {nameof(Type.GetInterfaces)} 的順序（穩定）。
]
""")]
	private static partial Type? FindGenericIface(
		Type T,
		Type Def
	);

	[Doc($"""
#Sum[收集成員：公開實例屬性（排除索引器）加公開實例字段。]

#Params([[T, 要收集的型別]])

#Rtn[成員表（尚未規整，由 {nameof(TypeInfoBase)} 建構子統一處理）]

#Descr[
屬性段在前、字段段後；
段內順序 = 收集序（即 {nameof(Type.GetProperties)} 與 {nameof(Type.GetFields)} 交出的順序，
它就是可用的穩定序；
不用 {nameof(MemberInfo.MetadataToken)}：NativeAOT 的 NativeFormat 元數據不提供它，取會拋）。

類型層次的合併（基類在前）與同名遮蔽的去重由 {nameof(TypeInfoSorter)} 統一處理。

實測（`PoUser`）：收到 11 項，屬性段 10 項加字段段 1 項（`Note`）；
同型別上的 `StaticNote`（靜態）、`Hidden`（私有）、`this[i32]`（索引器）都不收。
]
""")]
	private static partial IReadOnlyList<IMemberInfo> CollectMembers(
		Type T
	);

	[Doc($"""
#Sum[建立無參實例委託。]

#Params([[T, 要建實例的型別]])

#Rtn[無參實例工廠；無構造函數、抽象、接口返回 null]

#Descr[
值型別取默認值；類型別解析無參構造函數。

返回型別與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致。

JIT 下用表達式樹一次編譯成委託（靜態引用構造函數，剪裁友好）；
NativeAOT 不支持動態編譯
（{nameof(Expression)}.{nameof(Expression.Lambda)} 的 Compile 拋
{nameof(PlatformNotSupportedException)}），
退回 {nameof(Activator)}.{nameof(Activator.CreateInstance)}——
構造函數元數據已由 {nameof(ReflDam)} 保證保留。

實測：`typeof(PoUser)` 返回委託，調一次得到 `PoUser` 實例；
`typeof(PoNoCtor)`（只有帶參構造函數）返回 null，故其 {nameof(ITypeInfo.CanMkInst)} 為 false。
]
""")]
	private static partial Func<obj>? TryBuildMkInst(
		Type T
	);
}