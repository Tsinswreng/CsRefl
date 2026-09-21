namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[成員契約：成員名、宣告型別、成員型別、能力、讀寫——門面的成員層型別。]

#Descr[
官方兩側沒有共同的成員型別（{nameof(JsonPropertyInfo)} 不繼承 {nameof(MemberInfo)}），
故本包給成員一個契約；兩套來源各出一個配接器實現它：
{nameof(ReflMemberInfo)} 持官方 {nameof(MemberInfo)}（{nameof(PropertyInfo)}／{nameof(FieldInfo)}）、
{nameof(JsonMemberInfo)} 持官方 {nameof(JsonPropertyInfo)}。

實測（`PoUser`）：`Age` 的 {nameof(Name)} 是 "Age"、{nameof(PropertyType)} 是 `typeof(i32)`、可讀可寫。
]
""")]
public partial interface IMemberInfo{
	[Doc($$"""
#Sum[成員名（官方的名字，就是門面的鍵）。]

#Descr[
調用方這樣寫：

```csharp
var M = Src.GetInfo<PoUser>().GetMember(nameof(PoUser.Age));
M.Name;            // "Age"
M.PropertyType;    // typeof(i32)
M.DeclaringType;   // typeof(PoUser)
```
]
""")]
	str Name{get;}

	[Doc($"""
#Sum[成員的宣告型別（不是值的運行期型別）。]

#Descr[
實測（`PoUser`）：`Age` → `typeof(i32)`；字段 `Note` → `typeof(str)`。
]
""")]
	Type PropertyType{get;}

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
實測：`Age` → `typeof(PoUser)`；繼承成員 `Id` → `typeof(PoUserBase)`。
]
""")]
	Type DeclaringType{get;}

	[Doc($"""
#Sum[本成員可否讀取。]

#Descr[
實測（`PoUser`）：`Age` true；只讀的 `Secret` true；只寫的 `Token` false。
]
""")]
	bool CanRead{get;}

	[Doc($"""
#Sum[本成員可否寫入。]

#Descr[
實測（`PoUser`）：`Age` true；只讀的 `Secret` false；只寫的 `Token` true。
]
""")]
	bool CanWrite{get;}

	[Doc($"""
#Sum[官方特性提供者（兩側共有的官方接口 {nameof(ICustomAttributeProvider)}）。]

#Descr[
實測（`PoUser`）：`Level` 上標了 `[MyDemoAttr("優等級", 2)]`，反射側成員自身與 Json 側
{nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.AttributeProvider)} 都取回 1 個該特性。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;}

	[Doc($"""
#Sum[讀取實例上的本成員；失敗返回 false。]

#Params([[O, 實例；null 或與宣告型別不符時返回 false], [V, 讀出的值]])

#Descr[
實測（`PoUser`，`Age = 26`）：`TryGet(User, out var V)` 返回 true 且 `V` 是 boxed 的 `i32` 26；
傳 null 或傳 `new PoColor()` 都返回 false。
]
""")]
	bool TryGet(obj? O, out obj? V);

	[Doc($"""
#Sum[寫入實例上的本成員；失敗返回 false。]

#Params([[O, 實例；null 或與宣告型別不符時返回 false], [V, 要寫入的值]])

#Descr[
實測（`PoUser`）：`TrySet(User, 31)` 返回 true 且 `User.Age` 變成 31；只讀成員返回 false 且不改值；
值型別不符照常拋（那是調用方的 bug）。
]
""")]
	bool TrySet(obj? O, obj? V);
}




