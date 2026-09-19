namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ITypeInfo)} 是一個型別的元資料門面：型別分類、成員表、按名查成員、無參實例工廠、集合與字典的鍵值型別。]

#Descr[
兩套來源各自實現本接口，對外語義一致，
調用方不需要知道自己拿到的是哪一套：
{nameof(ReflTypeInfoSrc)} 走反射，
{nameof(JsonTypeInfoSrc)} 走官方源生成元資料。

實測（同一型別、兩套來源各查一次，以下每一項兩邊完全相同）：
{nameof(Kind)} 都是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
{nameof(Members)} 都是 11 個成員，順序都是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`；
{nameof(Members)}[2] 的 {nameof(IMemberInfo.Name)} 都是 "Age"、
{nameof(IMemberInfo.PropertyType)} 都是 `typeof(i32)`、
{nameof(IMemberInfo.DeclaringType)} 都是 `typeof(PoUser)`；
{nameof(TryGetMember)}("Age") 都命中且返回同一實例；
{nameof(TryGetMember)}("NoSuch") 都返回 false。

唯一不同的兩項（由來源能力決定，不是實現差異）：
反射源的 {nameof(Members)}[0].{nameof(IMemberInfo.Member)} 是 {nameof(PropertyInfo)}、{nameof(IMemberInfo.Json)} 為 null，
且 {nameof(Members)}[10]（`Note`，帶 `[JsonInclude]` 的字段）之 {nameof(IMemberInfo.MemberType)} 是
{nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
Json 源的 {nameof(Members)}[0].{nameof(IMemberInfo.Json)} 非 null、{nameof(IMemberInfo.Member)} 為 null，
且 `Note` 之 {nameof(IMemberInfo.MemberType)} 只能報
{nameof(MemberTypes)}.{nameof(MemberTypes.Property)}（官方不暴露 `IsProperty`）。
]

#Descr[
設計原則（與官方 API 的關係）：
官方有的概念一律沿用官方的名字與型別。
{nameof(Kind)} 就是官方 {nameof(JsonTypeInfoKind)}；
{nameof(ElementType)} 與 {nameof(KeyType)} 與官方同名同義；
無參實例工廠就是官方 {nameof(JsonTypeInfo.CreateObject)}；
Json 來源還可從 {nameof(Json)} 直接拿到官方 {nameof(JsonTypeInfo)} 本體。

官方沒有的只有「以字符串為鍵」這一件事：
{nameof(Members)} 與按名查成員、名清單。
]
""")]
public interface ITypeInfo{
	[Doc($"""
#Sum[本元資料對應的型別，與官方 {nameof(JsonTypeInfo.Type)} 同義。]

#Descr[
實測：查 `typeof(PoUser)` 得到的元資料，
其 {nameof(Type)} 與 `typeof(PoUser)` 是同一個 {nameof(Type)} 物件，
可用 `Info.{nameof(Type)} == typeof(PoUser)` 直接比較（`{nameof(ReferenceEquals)}` 亦為 true）。

兩套來源都一樣，故調用方不必區分來源。
]
""")]
	Type Type{get;}

	[Doc($"""
#Sum[型別分類，直接用官方 {nameof(JsonTypeInfoKind)}。]

#Descr[
實測取值（兩套來源一致）：

+ `typeof(str)`、`typeof(i32)`、`typeof(bool)`、`typeof(DateTime)`、`typeof(PoColor)`（枚舉）、`typeof(i32?)`
	→ {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}；
+ `typeof(PoUser)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
+ `typeof(List<str>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
+ `typeof(Dictionary<str, i32>)` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}；
+ `typeof(DateTime[])` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}
	（集合優先於元素型別的標量性，故不是 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}）。
]
""")]
	JsonTypeInfoKind Kind{get;}

	[Doc($"""
#Sum[全部成員，順序即契約序：基類在前、同類內按來源的宣告序。]

#Descr[
反射源取收集序；Json 源取官方 {nameof(JsonTypeInfo.Properties)} 的既有序；
契約由 {nameof(TypeInfoSorter)} 統一規整，故兩套來源給出同一個順序。

實測：查 `typeof(PoUser)` 得到 11 個成員，逐位對照如下。

+ {nameof(Members)}[0] 是 `Id`，其 {nameof(IMemberInfo.DeclaringType)} 是 `typeof(PoUserBase)`
	（繼承成員在前，這是「基類在前」的直接證據）；
+ {nameof(Members)}[1] 是 `Name`，同為基類宣告；
+ {nameof(Members)}[2] 是 `Age`，{nameof(IMemberInfo.DeclaringType)} 是 `typeof(PoUser)`；
+ {nameof(Members)}[7] 是 `Secret`（只讀），{nameof(Members)}[9] 是 `Token`（只寫）；
+ {nameof(Members)}[10] 是 `Note`（帶 `[JsonInclude]` 的字段）。

不在表內的名字（實測都不出現）：
靜態屬性 `StaticNote`、私有屬性 `Hidden`、索引器 `this[i32]`。
]

#Descr[
同名遮蔽已去重（實測，兩套來源結論相同）：
在「基類宣告 `Id`、`Name`，子類用 `new` 再宣告 `Id`，並新增 `Age`」的繼承鏈上，
查子類得到 3 個成員，依次是 `Name`（基類那份）、`Id`、`Age`；
`Id` 只出現一次，且按名查到的 `Id` 其 {nameof(IMemberInfo.DeclaringType)} 是子類
（離實例最近的那份宣告勝出，並佔用被遮蔽成員原先的位置）。

名字說明：官方這一側叫 {nameof(JsonTypeInfo.Properties)}（Json）
或 {nameof(Type)}.{nameof(Type.GetProperties)} 加 {nameof(Type)}.{nameof(Type.GetFields)}（反射），
兩者沒有共同名，故本門面自取 {nameof(Members)} 作為統一出口。
]
""")]
	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	//你上面Doc 也沒用nameof語法我想f12跳轉都跳不了
	IReadOnlyList<IMemberInfo> Members{get;}

	[Doc($"""
#Sum[按名（{nameof(IMemberInfo.Name)}）查成員。]

#Descr[
實測：

+ `Info.{nameof(TryGetMember)}("Age", out var M)` 返回 true，
	`M.{nameof(IMemberInfo.Name)}` 是 "Age"、`M.{nameof(IMemberInfo.PropertyType)}` 是 `typeof(i32)`，
	`M.{nameof(IMemberInfo.CanRead)}` 與 `M.{nameof(IMemberInfo.CanWrite)}` 都是 true；
+ `Info.{nameof(TryGetMember)}("NoSuch", out var Miss)` 返回 false 且 `Miss` 為 null（不拋）；
+ `Info.{nameof(TryGetMember)}("Secret", out var S)` 返回 true 但 `S.{nameof(IMemberInfo.CanWrite)}` 為 false；
+ `Info.{nameof(TryGetMember)}("Token", out var Tk)` 返回 true 但 `Tk.{nameof(IMemberInfo.CanRead)}` 為 false。

與 {nameof(GetMember)} 的關係（實測）：`{nameof(GetMember)}("Age")` 返回的實例
與本方法返回的實例 `{nameof(ReferenceEquals)}` 為 true，即共用同一份按名索引緩存。
]
""")]
//TswgTodo 你媽的 能不能好好寫示例??? 說了多少次了 註釋就放一堆乾巴巴的文字?
	bool TryGetMember(str Name, [NotNullWhen(true)] out IMemberInfo? M);

	[Doc($"""
#Sum[按名（{nameof(IMemberInfo.Name)}）取成員，取不到就拋。]

#Descr[
實測：

+ `Info.{nameof(GetMember)}("Age")` 返回 `Age` 的 {nameof(IMemberInfo)}；
+ `Info.{nameof(GetMember)}("NoSuch")` 拋 {nameof(KeyNotFoundException)}，
	訊息含可用成員清單（實測含 "Age" 這個字串可被斷言），形如
	「型別 ... 沒有成員 NoSuch。可用成員：Id, Name, Age, Email, Married, Tags, Extra, Secret, Level, Token, Note」，
	故拼錯名字時不必再自己去列 {nameof(Members)} 對照。

不確定名字在不在時用 {nameof(TryGetMember)}，那個只返回 false、不拋。
]
""")]
	IMemberInfo GetMember(str Name);

	[Doc($"""
#Sum[可讀成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）。

實測：`typeof(PoUser)` 的 {nameof(Members)} 依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`，
其中 `Secret` 只讀、`Token` 只寫、其餘 9 個可讀可寫，
故 {nameof(ReadableNames)} 的 {nameof(IReadOnlyCollection<int>)}.{nameof(IReadOnlyCollection<int>.Count)} 是 10，
內容依次為 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Note`。

注意判據是「這個成員能不能讀」，不是「在不在字典視圖的鍵表裏」：
只讀的 `Secret` 在可讀名裏（它是可讀的），只寫的 `Token` 不在可讀名裏。
]
""")]
	IReadOnlyCollection<str> ReadableNames{get;}

	[Doc($"""
#Sum[可寫成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）。

實測：接上例，{nameof(WritableNames)} 的 {nameof(IReadOnlyCollection<int>)}.{nameof(IReadOnlyCollection<int>.Count)} 也是 10，
內容依次為 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Token`、`Note`
（與 {nameof(ReadableNames)} 的差別只有一處：把 `Secret` 換成了 `Token`，且 `Token` 的位置與 {nameof(Members)} 同序）。

故兩份清單都不等於全部成員：只寫的 `Token` 只在可寫名裏，只讀的 `Secret` 只在可讀名裏。
]
""")]
	IReadOnlyCollection<str> WritableNames{get;}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.ElementType)} 同義。

實測取值：

+ `typeof(List<str>)` → `typeof(str)`，`typeof(i32[])` → `typeof(i32)`；
+ `typeof(Dictionary<str, i32>)` → `typeof(i32)`
	（字典取的是值型別，這一點與官方一致，不是鍵型別）；
+ `typeof(i32)`、`typeof(PoColor)` → null（不是集合）。
]
""")]
	Type? ElementType{get;}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.KeyType)} 同義。

實測取值：

+ `typeof(Dictionary<str, i32>)` → `typeof(str)`；
+ `typeof(List<str>)` → null（不是字典）；
+ 一個自身不是字典、但身上有個 `Dictionary<str, i32>` 型別成員的類 → null
	（元資料描述的是這個型別本身，不是它某個成員的型別）。

故本屬性非 null 時 {nameof(ElementType)} 也非 null，兩者由同一事實得出。
]
""")]
	Type? KeyType{get;}

	[Doc($"""
#Sum[無參實例工廠，型別與語義就是官方 {nameof(JsonTypeInfo.CreateObject)}。]

#Descr[
null 表示本型別不能建無參實例。反射來源同樣提供這個形狀。

實測：

+ `typeof(PoUser)`（有公開無參構造函數）→ 非 null，調一次得到一個 `PoUser`；
+ `typeof(PoNoCtor)`（只有帶參構造函數）→ null；
+ `typeof(i32)` → 非 null，造的是一個 boxed 的 0（值型別不必有構造函數）。

只是為了判斷「能不能建實例」時查 {nameof(CanMkInst)} 即可，不必取本屬性再判空。
]
""")]
	Func<obj>? CreateObject{get;}

	[Doc($"""
#Sum[被包裝的官方 {nameof(JsonTypeInfo)} 本體；反射來源為 null。]

#Descr[
調用方要用官方元資料能力時直接從這裡拿，
例如官方 {nameof(JsonTypeInfo.Properties)}、官方 `UnmappedMemberHandling`、
`NumberHandling`、`PolymorphismOptions`。

實測：從 {nameof(JsonTypeInfoSrc)} 取回的元資料，
`Info.{nameof(Json)}!.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;`
這一行照官方語義生效；
從 {nameof(ReflTypeInfoSrc)} 取回的同一個型別，
`Info.{nameof(Json)}` 是 null（反射側沒有官方元資料本體），
此時官方能力不可用，只能走本接口自己的成員表。
]
""")]
	JsonTypeInfo? Json{get;}

	[Doc($"""
#Sum[本型別能否建立無參實例。]

#Descr[
自研便利，判據就是官方那條「{nameof(CreateObject)} 是否為 null」，故兩者恆同步。

實測：`typeof(PoUser)`（有公開無參構造函數）為 true；
`typeof(PoNoCtor)`（只有帶參構造函數）為 false。

只是為了判斷要不要走建實例那條路時查本屬性，免得先取 {nameof(CreateObject)} 再判空、還要處理可空性警告。
]
""")]
	bool CanMkInst{get;}

	[Doc($"""
#Sum[建立一個無參實例，內部轉調 {nameof(CreateObject)}。]

#Descr[
實測：`var U = Info.{nameof(MkInst)}()` 得到一個 `PoUser` 新實例，
其 `Id` 可立即賦值（`U.Id = 3` 之後讀回 3），
故拿到手的是一個可用的空殼，之後可用 `Info.{nameof(GetMember)}("Age")`
配 {nameof(IMemberInfo.TrySet)} 往裏填值。

不可建時拋 {nameof(NotSupportedException)}（訊息含型別全名）。
先查 {nameof(CanMkInst)} 可以避免這個異常。
]
""")]
	obj? MkInst();
}