namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[成員的統一操作：成員物件是官方 {nameof(MemberInfo)}（反射側）或官方 {nameof(JsonPropertyInfo)}（Json 側），本類只補官方沒有的那幾件事。]

#Descr[
為甚麼是擴展方法而不是包裝型別：
官方成員物件本身已經把成員事實給全了——
{nameof(JsonPropertyInfo)} 有 {nameof(JsonPropertyInfo.Name)}、{nameof(JsonPropertyInfo.PropertyType)}、
{nameof(JsonPropertyInfo.DeclaringType)}、{nameof(JsonPropertyInfo.Get)}、{nameof(JsonPropertyInfo.Set)}、
{nameof(JsonPropertyInfo.AttributeProvider)}；
{nameof(MemberInfo)} 有 {nameof(MemberInfo.Name)}、{nameof(MemberInfo.DeclaringType)}、{nameof(MemberInfo.MemberType)}，
{nameof(PropertyInfo)} 另有 {nameof(PropertyInfo.PropertyType)} 與 {nameof(PropertyInfo.GetValue)}／{nameof(PropertyInfo.SetValue)}。

把這些再抄進本包自己的型別（本包先前的 `IMemberInfo` 就是那樣，還多出 `Member`／`Json` 兩個判位），
等於把官方 API 重寫一遍，且逼每個調用方先分辨來源。

故改為：官方物件原樣交出去，本類只補官方沒有的三件事——
取名字與宣告型別的統一入口、可讀／可寫的統一判據、
以及實例可空、型別不符時返回 false 的 {nameof(TryGet)}／{nameof(TrySet)}。
]
""")]
public static partial class MemberExtn{
	[Doc($"""
#Sum[成員名。]

#Descr[
官方 {nameof(MemberInfo.Name)} 或官方 {nameof(JsonPropertyInfo.Name)}，不另存副本。

實測（`PoUser`）：`Age` 兩側都是 "Age"、`Note` 兩側都是 "Note"。
]
""")]
	public static partial str Name(this obj? M);

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
實測（`PoUser`）：`Age` 兩側都是 `typeof(PoUser)`；
繼承成員 `Id` 兩側都是 `typeof(PoUserBase)`。
]
""")]
	public static partial Type? DeclaringType(this obj? M);

	[Doc($"""
#Sum[本成員可否讀取。]

#Descr[
兩側都直接用官方自己的那條判據，不引入第二套口徑：
Json 側看官方 {nameof(JsonPropertyInfo.Get)} 是否為 null；
反射側看有沒有公開訪問器（{nameof(PropertyInfo.GetGetMethod)}），
不用 {nameof(PropertyInfo.CanRead)}——屬性帶私有 get 時它仍為 true，會把只寫屬性誤判成可讀。

實測（`PoUser`）：`Age` 兩側都是 true；只讀的 `Secret` 兩側都是 true；只寫的 `Token` 兩側都是 false。
]
""")]
	public static partial bool CanRead(this obj? M);

	[Doc($"""
#Sum[本成員可否寫入。]

#Descr[
Json 側看官方 {nameof(JsonPropertyInfo.Set)} 是否為 null；
反射側看 {nameof(PropertyInfo.GetSetMethod)} 或 {nameof(FieldInfo.IsInitOnly)}。

實測（`PoUser`）：`Age` 兩側都是 true；只讀的 `Secret` 兩側都是 false；只寫的 `Token` 兩側都是 true。
]
""")]
	public static partial bool CanWrite(this obj? M);

	[Doc($"""
#Sum[讀取實例上的本成員；失敗返回 false。]

#Params([[M, 官方成員物件], [O, 實例；null 或與宣告型別不符時返回 false], [V, 讀出的值；失敗時為 default]])

#Rtn[不可讀、實例為 null 或型別不符時返回 false]

#Descr[
實測（`PoUser`，`Age = 30`）：兩側都讀出 boxed 的 `i32` 30；
只讀的 `Secret` 兩側都讀出 "s"；只寫的 `Token` 兩側都返回 false；傳 null 返回 false；
傳 `new PoColor()` 返回 false（型別不符）。
]
""")]
	public static partial bool TryGet(this obj? M, obj? O, out obj? V);

	[Doc($"""
#Sum[寫入實例上的本成員；失敗返回 false。]

#Params([[M, 官方成員物件], [O, 實例；null 或與宣告型別不符時返回 false], [V, 要寫入的值]])

#Rtn[不可寫、實例為 null 或型別不符時返回 false]

#Descr[
值型別不符不返回 false，而是照常拋（那是調用方的 bug，不是「不可寫」）。

實測（`PoUser`）：`Age` 兩側都寫得進；繼承成員 `Name` 兩側都寫得進；
只讀的 `Secret` 兩側都返回 false 且不動實例。
]
""")]
	public static partial bool TrySet(this obj? M, obj? O, obj? V);
}