namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`IMemberInfo` 的擴展：特性查詢。]

#Descr[
為甚麼需要這一層：
官方把 `GetCustomAttribute<T>()` 只掛在具體型別上
（`MemberInfo`／`Assembly`／`Module`／`ParameterInfo`／`PropertyInfo`），
而官方在 Json 與反射兩側*共有*的接口 `ICustomAttributeProvider` 上並沒有它。

門面的 `AttributeProvider` 正是 `ICustomAttributeProvider`，
故補一個與官方同形的便利方法
（名字、返回 null 的約定都照官方 `CustomAttributeExtensions`），
讓調用方不必自己寫 `GetCustomAttributes(typeof(T), false)` 的樣板。

實現見 `IMemberInfoExtn.Impl.cs`。
]
""")]
public static partial class IMemberInfoExtn{
	[Doc("""
#Sum[取本成員上第一個 `TAttr` 型別的特性；沒有返回 null。]

#TParams[要取的特性型別；必須是 `Attribute` 的子類]

#Params([[成員元資料；為 null 時返回 null]])

#Rtn[取到的特性；沒有則為 null]

#Descr[
與官方 `CustomAttributeExtensions.GetCustomAttribute<T>` 同語義：
不繼承、不拋。

實現直接轉官方 `ICustomAttributeProvider.GetCustomAttributes`，
第二個實參 false 與官方一致（不繼承）。
]
""")]
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute;
}