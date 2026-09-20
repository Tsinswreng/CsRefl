namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[特性查詢：官方在兩側共有的接口 {nameof(ICustomAttributeProvider)} 上沒有便利方法，故補一個。]

#Descr[
為甚麼需要這一層：
官方把 {nameof(CustomAttributeExtensions.GetCustomAttribute)} 只掛在具體型別上
（{nameof(MemberInfo)}、{nameof(Assembly)}、{nameof(Module)}、{nameof(ParameterInfo)}、{nameof(PropertyInfo)}），
而兩側共有的官方接口 {nameof(ICustomAttributeProvider)} 上並沒有它。

故直接對官方的 {nameof(ICustomAttributeProvider)} 做擴展
（反射側成員本身就是它，Json 側取官方 {nameof(JsonPropertyInfo.AttributeProvider)}），
名字與「取不到返回 null」的約定都照官方。

實現見 `AttrProvider.Impl.cs`。
]
""")]
public static partial class AttrProvider{
	[Doc($"""
#Sum[取本提供者上第一個 `TAttr` 型別的特性；沒有返回 null。]

#TParams[要取的特性型別；必須是 {nameof(Attribute)} 的子類]

#Params([[z, 官方特性提供者；為 null 時返回 null]])

#Rtn[取到的特性；沒有則為 null]

#Descr[
與官方 {nameof(CustomAttributeExtensions.GetCustomAttribute)} 同語義：不繼承、不拋；
實現直接轉官方 {nameof(ICustomAttributeProvider)}.{nameof(ICustomAttributeProvider.GetCustomAttributes)}，
第二個實參 false 與官方一致。

實測（`PoUser`）：`Level` 上標了 `[MyDemoAttr("優等級", 2)]`，
反射側（成員自身）與 Json 側（官方 {nameof(JsonPropertyInfo.AttributeProvider)}）
都取回 1 個該特性（`Tag` 是 "優等級"、`Rank` 是 2）；
`Age` 上沒標故返回 null。
]
""")]
	public static partial TAttr? GetCustomAttribute<TAttr>(ICustomAttributeProvider? Provider) where TAttr:Attribute;
}


