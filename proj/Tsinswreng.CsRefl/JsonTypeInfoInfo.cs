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
	[Doc($$"""
#Sum[用官方型別元資料建配接器。]

#Params([[Json, 官方型別元資料；不允許 null]])

#Descr[
調用方通常不直接 new，而是從 {{nameof(JsonTypeInfoSrc)}} 拿；
要自己包時這樣寫：

```csharp
var Json = TestJsonCtx.Default.GetTypeInfo(typeof(PoUser))!;
var Info = new JsonTypeInfoInfo(Json);

Info.Kind;            // JsonTypeInfoKind.Object
Info.Members.Count;   // 11，每一項都是官方 JsonPropertyInfo
Info.GetMember(nameof(PoUser.Age));

new JsonTypeInfoInfo(null!);
// 拋 ArgumentNullException（構造期就擋住，不留到查詢時）。
```

標量與集合沒有成員：包 `typeof(List<str>)` 時 {{nameof(ITypeInfo.Members)}} 為空、
{{nameof(ITypeInfo.ElementType)}} 是 `typeof(str)`。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json);

	[Doc($"""
#Sum[無參實例工廠；每次現讀官方本體那條委託。]

#Descr[
這裡刻意是「現讀」而不是構造期快照：官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)}
是可寫的，官方本體被換掉時門面要跟著變，才有資格叫薄配接器。

實測：包裝完成後把官方 {nameof(JsonTypeInfo.CreateObject)} 換成別的委託，
本屬性的取值立刻跟著變（與換上去的那個委託 `{nameof(ReferenceEquals)}` 為 true）。

賦值＝覆蓋這條委託；本輪只加形狀，實現待寫。
]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return Json?.CreateObject;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本屬性是現讀官方本體那條委託，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[官方型別元資料本體；構造期賦值，可再賦值。]

#Descr[
就是構造時傳進來的那個官方實例（{nameof(ReferenceEquals)} 為 true），
{nameof(Members)} 與 {nameof(CreateObject)} 都以它為源。
]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public override JsonTypeInfo? Json{
		get;
		set;
	}

	[Doc($"""
#Sum[建立無參實例；官方沒有工廠時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();
}




