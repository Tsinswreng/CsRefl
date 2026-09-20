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

	[Doc($$"""
#Sum[讀取實例上的本成員；失敗返回 false。]

#Params([[M, 官方成員物件], [O, 實例；null 或與宣告型別不符時返回 false], [V, 讀出的值；失敗時為 default]])

#Rtn[不可讀、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo(typeof(PoUser));
var User = new PoUser{ Age = 26 };

var M = Info.GetMember(nameof(PoUser.Age));
M.TryGet(User, out var V);
// true；V 是 boxed 的 i32 26。

Info.GetMember(nameof(PoUser.Secret)).TryGet(User, out var S);   // true；V 是 "s"（只讀成員讀得到）
Info.GetMember(nameof(PoUser.Token)).TryGet(User, out _);        // false：只寫成員讀不到
Info.GetMember(nameof(PoUser.Age)).TryGet(null, out _);          // false：實例是 null
Info.GetMember(nameof(PoUser.Age)).TryGet(new PoColor(), out _); // false：實例與宣告型別不符
```

兩套來源同一條口徑：反射側的 `PropertyInfo`/`FieldInfo` 與 Json 側的 `JsonPropertyInfo` 都走本方法。
]
""")]
	public static partial bool TryGet(this obj? M, obj? O, out obj? V);

	[Doc($$"""
#Sum[寫入實例上的本成員；失敗返回 false。]

#Params([[M, 官方成員物件], [O, 實例；null 或與宣告型別不符時返回 false], [V, 要寫入的值]])

#Rtn[不可寫、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo(typeof(PoUser));
var User = new PoUser{ Age = 26 };

Info.GetMember(nameof(PoUser.Age)).TrySet(User, 31);
// true；之後 User.Age 是 31。

Info.GetMember(nameof(PoUser.Name)).TrySet(User, "阿強");
// true；繼承來的成員一樣寫得進。

Info.GetMember(nameof(PoUser.Secret)).TrySet(User, "x");
// false：Secret 是只讀成員，且 User.Secret 仍是 "s"（不動實例）。

Info.GetMember(nameof(PoUser.Age)).TrySet(User, "不是數字");
// 拋（不是返回 false）：值型別不符是調用方的 bug。
```
]
""")]
	public static partial bool TrySet(this obj? M, obj? O, obj? V);

	[Doc($$"""
#Sum[成員的宣告型別；未知物件返回 null。]

#Params([[M, 官方成員物件；null 或認不得的型別返回 null]])

#Rtn[宣告型別；取不到為 null]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();

MemberExtn.PropertyType(Info.GetMember(nameof(PoUser.Age)));    // typeof(i32)
MemberExtn.PropertyType(Info.GetMember(nameof(PoUser.Note)));   // typeof(str)：Note 是字段
MemberExtn.PropertyType(Info.GetMember(nameof(PoUser.Tags)));   // typeof(List<str>)
MemberExtn.PropertyType("不是成員");                            // null
```

兩側一條口徑：{{nameof(PropertyInfo)}} 與 {{nameof(JsonPropertyInfo)}} 取 `PropertyType`、
{{nameof(FieldInfo)}} 取 `FieldType`，故調用方不必自己分辨來源與成員種類。
]
""")]
	public static partial Type? PropertyType(this obj? M);
}
