namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[JsonTypeInfo 來源的型別元資料：包一個非泛型 {nameof(JsonTypeInfo)}。]

#Descr[
官方本體直接對外暴露（{nameof(Json)}），需要甚麼官方能力直接從那裡拿；
{nameof(Kind)} 直接就是官方 {nameof(JsonTypeInfoKind)}，無需映射。
成員沿用 {nameof(JsonTypeInfo.Properties)} 的既有序。

例：要改官方反序列化行為（例如不許未對應的成員）時，
`Info.{nameof(Json)}!.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;`
這一行就直接作用在官方元資料上，本包不另做一層轉譯。
]

#Descr[
建構子與 {nameof(MkInst)} 實現見 `JsonTypeInfoInfo.Impl.cs`。
]
""")]
public partial class JsonTypeInfoInfo:TypeInfoBase{
	[Doc($"""
#Sum[被包的官方 {nameof(JsonTypeInfo)}。]

#Descr[
例：{nameof(CreateObject)} 與 {nameof(Json)} 兩個成員都直接轉發這個字段，
故包裝層不持有第二份狀態，官方改了它也就跟著變。
]
""")]
	private readonly JsonTypeInfo _json;

	[Doc($"""
#Sum[包一個 {nameof(JsonTypeInfo)}。]

#Params([[要包的官方型別元資料]])

#Descr[
例：從 {nameof(JsonTypeInfoSrc)} 查到的元資料內部就是這樣包出來的，
成員表在建構子裏由官方 {nameof(JsonTypeInfo.Properties)} 轉成 {nameof(IMemberInfo)}，
再交給 {nameof(TypeInfoBase)} 規整成契約序。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json);

	[Doc($"""
#Sum[無參實例工廠，直接轉官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)}。]

#Descr[
官方就是用它表示「可不可以建實例」（標量等型別官方給 null）。

例：掛了 `[JsonSerializable]` 的類別非 null，`i32` 之類的標量官方給 null，
故本包不必自己判斷可建性，轉發即可。
]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return _json.CreateObject;
		}
	}

	[Doc($"""
#Sum[被包的官方 {nameof(JsonTypeInfo)} 本體。]

#Descr[
例：`Info.{nameof(Json)}!.{nameof(JsonTypeInfo.Type)}` 與 `Info.{nameof(Type)}` 是同一個 {nameof(Type)}，
兩處取到的東西一致，故從哪邊拿都不會出現分歧。
]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public override JsonTypeInfo Json{
		get{
			return _json;
		}
	}

	[Doc($"""
#Sum[建立實例，轉調 {nameof(CreateObject)} 工廠；無無參工廠時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();
}