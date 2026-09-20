namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[官方 {nameof(JsonTypeInfo)} 的薄配接器：把官方元資料接到 {nameof(ITypeInfo)} 上。]

#Descr[
薄的定義：凡是官方已有的，一律原樣轉發、一個字不算——
{nameof(ITypeInfo.Type)} 取官方 {nameof(JsonTypeInfo.Type)}、
{nameof(ITypeInfo.Kind)} 取官方 {nameof(JsonTypeInfo.Kind)}、
{nameof(ITypeInfo.Members)} 取官方 {nameof(JsonTypeInfo.Properties)}、
{nameof(ITypeInfo.ElementType)} 與 {nameof(ITypeInfo.KeyType)} 同名同義、
{nameof(ITypeInfo.CreateObject)} 直接就是官方那條委託。
按名索引與名清單緩存由 {nameof(TypeInfoBase)} 提供，本類不重複寫一遍。

為甚麼需要這一層：官方 {nameof(JsonTypeInfo)} 是官方的類，本包改不了它的宣告，
而源生成來源必須交出一份 {nameof(ITypeInfo)}。

實測：`typeof(PoUser)` 經本類包裝後，{nameof(Members)} 的每一項都是官方 {nameof(JsonPropertyInfo)}
（首項是 `Id`、末項是 `Note`），{nameof(Json)} 就是傳進來的那個官方實例
（{nameof(ReferenceEquals)} 為 true）。

建構子與 {nameof(MkInst)} 的實現見 `JsonTypeInfoInfo.Impl.cs`。
]
""")]
public partial class JsonTypeInfoInfo:TypeInfoBase{
	[Doc($"""
#Sum[官方型別元資料本體；對外由 {nameof(ITypeInfo.Json)} 原樣交出。]

#Descr[
不做任何拷貝：{nameof(CreateObject)}、{nameof(Members)} 的來源都是它。
]
""")]
	private readonly JsonTypeInfo _json;

	[Doc($"""
#Sum[用官方型別元資料建配接器。]

#Params([[Json, 官方型別元資料；不允許 null]])

#Descr[
實測：`typeof(List<str>)` 的官方元資料包進來後，
{nameof(ITypeInfo.Kind)} 是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}、
{nameof(ITypeInfo.ElementType)} 是 `typeof(str)`、{nameof(ITypeInfo.Members)} 為空（標量與集合都沒有成員）；
`typeof(PoUser)` 的則是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)} 與 11 個成員。

`Json` 傳 null 在構造期拋 {nameof(ArgumentNullException)}。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json);

	[Doc($"""
#Sum[無參實例工廠；直接轉官方那條委託。]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return _json.CreateObject;
		}
	}

	[Doc($"""
#Sum[官方本體，原樣交出。]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public override JsonTypeInfo? Json{
		get{
			return _json;
		}
	}

	[Doc($"""
#Sum[建立無參實例；官方沒有工廠時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();
}
