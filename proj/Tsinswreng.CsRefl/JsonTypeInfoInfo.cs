namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[JsonTypeInfo 來源的型別元資料：包一個非泛型 {nameof(JsonTypeInfo)}。]

#Descr[
官方本體直接對外暴露（{nameof(Json)}），需要甚麼官方能力直接從那裡拿；
{nameof(Kind)} 直接就是官方 {nameof(JsonTypeInfoKind)}，無需映射。
成員沿用 {nameof(JsonTypeInfo.Properties)} 的既有序。

實測：`Info.{nameof(Json)}` 拿到的就是官方那個 {nameof(JsonTypeInfo)} 本體，
故要改官方反序列化行為（例如不許未對應的成員）時，
`Info.{nameof(Json)}!.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;`
這一行就直接作用在官方元資料上，本包不另做一層轉譯。

實測（`PoUser`）：`Info.{nameof(Json)}!.{nameof(JsonTypeInfo.Type)}` 是 `typeof(PoUser)`；
官方 {nameof(JsonTypeInfo.Properties)} 的項數與 `Info.{nameof(Members)}.Count` 都是 11
（兩者口徑必須一致，因為成員表就是從它轉出來的）；
從反射來源取回的同一個型別，`Info.{nameof(Json)}` 是 null。
]

#Descr[
建構子與 {nameof(MkInst)} 實現見 `JsonTypeInfoInfo.Impl.cs`。
]
""")]
public partial class JsonTypeInfoInfo:TypeInfoBase{
	
	
	[Doc($"""
#Sum[被包的官方 {nameof(JsonTypeInfo)}。]

#Descr[
實測：`Info.{nameof(Json)}` 就是這個字段本身（`{nameof(ReferenceEquals)}` 為 true）；
{nameof(CreateObject)} 與 {nameof(Json)} 兩個成員都直接轉發這個字段，
故包裝層不持有第二份狀態，官方改了它也就跟著變。

實測：`Info.{nameof(Json)}` 與 `{nameof(JsonTypeInfoInfo)}` 內部的這個字段是同一實例
（`{nameof(ReferenceEquals)}` 為 true），沒有第二份包裝物件。
]
""")]


	public partial JsonTypeInfoInfo(JsonTypeInfo Json);


	[Doc($"""
#Sum[無參實例工廠，直接轉官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)}。]

#Descr[
官方就是用它表示「可不可以建實例」（標量等型別官方給 null）。

實測：`typeof(PoUser)` 非 null，調一次得到一個 `PoUser`；
`typeof(PoNoCtor)`（未註冊且無無參構造函數）為 null，
故本包不必自己判斷可建性，轉發即可。
]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public override Func<obj>? CreateObject{
		get{
			return Json.CreateObject;
		}
	}

	[Doc($"""
#Sum[被包的官方 {nameof(JsonTypeInfo)} 本體。]

#Descr[
實測（`PoUser`）：`Info.{nameof(Json)}!.{nameof(JsonTypeInfo.Type)}` 與 `Info.{nameof(Type)}` 是同一個 {nameof(Type)}（`{nameof(ReferenceEquals)}` 為 true），
兩處取到的東西一致，故從哪邊拿都不會出現分歧。
]

#See[{nameof(ITypeInfo.Json)}]
""")]
	
	public override JsonTypeInfo Json{get;}

	[Doc($"""
#Sum[建立實例，轉調 {nameof(CreateObject)} 工廠；無無參工廠時拋 {nameof(NotSupportedException)}。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst();

	// ---- 私有輔助（實現見 JsonTypeInfoInfo.Impl.cs）----

	[Doc($"""
#Sum[把官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.Properties)} 包成成員表。]

#Params([[Json, 官方型別元資料]])

#Rtn[包好的成員表（尚未規整，由 {nameof(TypeInfoBase)} 建構子統一處理）]

#Descr[
官方 {nameof(JsonPropertyInfo.Name)} 是 JSON 名，
與 C# 名相等的前提由調用方保證（命名策略為 null 且無 `[JsonPropertyName]`）。

實測（`PoUser`）：官方有 11 個成員時這裡就轉出 11 個 {nameof(JsonMemberInfo)}，
順序保持官方既有序，之後由 {nameof(TypeInfoSorter)} 規整。
]
""")]
	private static partial IReadOnlyList<IMemberInfo> CollectMembers(JsonTypeInfo Json);
}
