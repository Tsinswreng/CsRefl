namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[門面的擴展：官方沒有的「以字符串為鍵」的全部操作。]

#Descr[
官方在兩側各有一套成員體系，卻沒有共同的按名查詢：
反射側要自己 {nameof(Type)}.{nameof(Type.GetProperties)} 加 {nameof(Type)}.{nameof(Type.GetFields)} 再比名字，
Json 側要自己掃官方 {nameof(JsonTypeInfo.Properties)}。
故本類只補這一件事與它衍生的幾項，成員事實一律回官方物件取，不另存副本。

按名讀寫的判據是「成員自身能力」，不是「在不在某張鍵表裏」：
只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。

實現見 `ITypeInfoExtn.Impl.cs`。
]
""")]
public static partial class ITypeInfoExtn{
	[Doc($"""
#Sum[按名（成員的官方名字）查成員。]

#Params([[z, 型別元資料], [Name, 成員名], [M, 查到的官方成員物件；未命中為 null]])

#Rtn[命中返回 true；未命中返回 false]

#Descr[
命中的 {nameof(M)} 是官方成員物件：反射側是 {nameof(MemberInfo)}、Json 側是 {nameof(JsonPropertyInfo)}；
兩側取名字一律走 {nameof(MemberExtn.Name)}，不必自己分辨型別。

實測（`PoUser`，兩套來源一致）：

+ `{nameof(TryGetMember)}(Info, "Age", out var M)` 返回 true，
	`{nameof(MemberExtn.Name)}(M)` 是 "Age"、
	`{nameof(MemberExtn.DeclaringType)}(M)` 是 `typeof(PoUser)`、
	`{nameof(MemberExtn.CanRead)}(M)` 與 `{nameof(MemberExtn.CanWrite)}(M)` 都是 true；
+ `"NoSuch"` 返回 false 且 `M` 為 null（不拋）；
+ `"Token"`（只寫）返回 true 但 `{nameof(MemberExtn.CanRead)}(M)` 為 false。

`{nameof(z)}` 為 null 時返回 false 而不拋，故適合接外部傳來的名字。
]
""")]
//TswgTodo 你媽的 能不能好好寫示例??? 說了多少次了 註釋就放一堆乾巴巴的文字?
	public static partial bool TryGetMember(this ITypeInfo z, str Name, [NotNullWhen(true)] out obj? M);

	[Doc($"""
#Sum[按名取成員；取不到就拋。]

#Params([[z, 型別元資料], [Name, 成員名]])

#Rtn[命中的官方成員物件]

#Descr[
實測：`{nameof(GetMember)}(Info, "Age")` 返回 `Age` 的官方成員物件，
與 {nameof(TryGetMember)} 命中時返回的是同一個實例；
`"NoSuch"` 拋 {nameof(KeyNotFoundException)}，
訊息含可用成員清單（實測含 "Age" 這個子串可被斷言），故拼錯名字時不必自己去列成員。

與 {nameof(TryGetMember)} 的關係（實測）：{nameof(GetMember)}(Info, "Age") 與
{nameof(TryGetMember)}(Info, "Age", out var M) 返回的實例 {nameof(ReferenceEquals)} 為 true，
即共用同一份按名索引緩存。
]
""")]
	public static partial obj? GetMember(this ITypeInfo z, str Name);

	[Doc($"""
#Sum[按名讀值。]

#Params([[z, 型別元資料], [Name, 成員名], [O, 實例], [V, 讀出的值；失敗時為 default]])

#Rtn[成員不存在、不可讀、實例為 null 或型別不符時返回 false]

#Descr[
實測（`PoUser`，`Age = 30`、`Secret = "s"`）：

+ `{nameof(TryGet)}(Info, "Age", User, out var V)` 返回 true 且 `V` 是 boxed 的 `i32` 30；
+ `"Secret"`（只讀）返回 true 且 `V` 是 "s"；
+ `"Token"`（只寫）返回 false；`"NoSuch"` 返回 false；傳 null 實例返回 false。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, str Name, obj? O, out obj? V);

	[Doc($"""
#Sum[按名寫值。]

#Params([[z, 型別元資料], [Name, 成員名], [O, 實例], [V, 要寫入的值]])

#Rtn[成員不存在、不可寫、實例為 null 或型別不符時返回 false]

#Descr[
值型別不符不返回 false，而是照常拋（那是調用方的 bug，不是「不可寫」）。

實測（`PoUser`）：`{nameof(TrySet)}(Info, "Age", User, 31)` 返回 true 且之後 `User.Age` 是 31；
`"Secret"`（只讀）返回 false 且不動實例；拿 `str` 當 `i32` 寫拋 {nameof(InvalidOperationException)}。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, str Name, obj? O, obj? V);

	[Doc($"""
#Sum[可讀成員名清單，順序同 {nameof(ITypeInfo.Members)}。]

#Descr[
自研便利（官方無此物）。

實測（`PoUser`）：10 個名，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Note`
（只讀的 `Secret` 在、只寫的 `Token` 不在，因為判據是「能不能讀」）。

用途：要把物件序列化成一行 SQL 或一份前端表單時直接遍歷它，不必自己過濾成員。
]
""")]
	public static partial IReadOnlyCollection<str> ReadableNames(this ITypeInfo z);

	[Doc($"""
#Sum[可寫成員名清單，順序同 {nameof(ITypeInfo.Members)}。]

#Descr[
自研便利（官方無此物）。

實測（`PoUser`）：10 個名，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Token`、`Note`
（只寫的 `Token` 在、只讀的 `Secret` 不在）。

用途：要按外部字典回填物件時，先拿這個清單擋掉不該寫的鍵。
]
""")]
	public static partial IReadOnlyCollection<str> WritableNames(this ITypeInfo z);

	[Doc($"""
#Sum[本型別能否建立無參實例。]

#Descr[
自研便利，判據就是官方那條「有沒有無參工廠」：
Json 側看官方 {nameof(JsonTypeInfo.CreateObject)}，反射側看反射算出的工廠。

實測：`typeof(PoUser)` 兩套來源都是 true；`typeof(PoNoCtor)`（只有帶參構造函數）兩套都是 false。
]
""")]
	public static partial bool CanMkInst(this ITypeInfo z);

	[Doc($"""
#Sum[建立一個無參實例；不可建時拋。]

#Rtn[新實例]

#Descr[
實測：`{nameof(MkInst)}(Info)` 得到一個 `PoUser` 新實例，其 `Id` 可立即賦值；
不可建時（如 `PoNoCtor`）拋 {nameof(NotSupportedException)}，訊息含型別全名。
先查 {nameof(CanMkInst)} 可以避免這個異常。
]
""")]
	public static partial obj? MkInst(this ITypeInfo z);
}