namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(IMemberInfo)} 的擴展：特性查詢。]

#Descr[
為甚麼需要這一層：
官方把 {nameof(CustomAttributeExtensions.GetCustomAttribute)} 只掛在具體型別上
（{nameof(System.Reflection.MemberInfo)}、{nameof(Assembly)}、
{nameof(System.Reflection.Module)}、{nameof(ParameterInfo)}、{nameof(PropertyInfo)}），
而官方在 Json 與反射兩側*共有*的接口 {nameof(ICustomAttributeProvider)} 上並沒有它。

門面的 {nameof(IMemberInfo.AttributeProvider)} 正是 {nameof(ICustomAttributeProvider)}，
故補一個與官方同形的便利方法
（名字、返回 null 的約定都照官方 {nameof(CustomAttributeExtensions)}），
讓調用方不必自己寫 `{nameof(ICustomAttributeProvider.GetCustomAttributes)}(typeof(T), false)` 的樣板。

例：不用本方法就得自己 `foreach` 官方返回的 {nameof(Array)} 再逐個判型別；
用了本方法一行就能拿到強型別結果，`null` 同時代表「沒標」與「標了但型別不匹配」。

實現見 `IMemberInfoExtn.Impl.cs`。
]
""")]
public static partial class IMemberInfoExtn{
	[Doc($"""
#Sum[取本成員上第一個 `TAttr` 型別的特性；沒有返回 null。]

#TParams[要取的特性型別；必須是 {nameof(Attribute)} 的子類]

#Params([[成員元資料；為 null 時返回 null]])

#Rtn[取到的特性；沒有則為 null]

#Descr[
與官方 {nameof(CustomAttributeExtensions.GetCustomAttribute)} 同語義：
不繼承、不拋。

實現直接轉官方 {nameof(ICustomAttributeProvider)}.{nameof(ICustomAttributeProvider.GetCustomAttributes)}，
第二個實參 false 與官方一致（不繼承）。

例：`Info.{nameof(ITypeInfo.GetMember)}("Level").{nameof(GetCustomAttribute)}<MyDemoAttr>()`
取得到就返回那個特性實例，取不到（成員上沒標）返回 null；
調用方不需要判 null 之後再判型別，本方法已把「不匹配」與「不存在」都歸成 null。

注意：這是便利方法，不是能力擴展，
故特性只在 {nameof(IMemberInfo.AttributeProvider)} 上取，兩套來源都走同一條路。
]
""")]
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute;
}