namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ITypeInfo)} 是一個型別的元資料門面：型別分類、成員表、按名查成員、無參實例工廠、集合/字典的鍵值型別。]

#Descr[
兩套來源各自實現本接口，對外語義一致，
調用方不需要知道自己拿到的是哪一套：
{nameof(ReflTypeInfoSrc)} 走反射，
{nameof(JsonTypeInfoSrc)} 走官方源生成元資料。

例：同一個型別分別從兩套來源取回元資料，
{nameof(ITypeInfo.Kind)}、{nameof(ITypeInfo.Members)} 的內容與順序都相同，
故調用方可以只寫一份邏輯，靠來源切換即可。
]

#Descr[
設計原則（與官方 API 的關係）：
官方有的概念一律沿用官方的名字與型別。
{nameof(ITypeInfo.Kind)} 就是官方 {nameof(JsonTypeInfoKind)}；
{nameof(ITypeInfo.ElementType)} 與 {nameof(ITypeInfo.KeyType)} 與官方同名同義；
無參實例工廠就是官方 {nameof(JsonTypeInfo.CreateObject)}；
Json 來源還可從 {nameof(ITypeInfo.Json)} 直接拿到官方 {nameof(JsonTypeInfo)} 本體。

官方沒有的只有「以字符串為鍵」這一件事：
{nameof(ITypeInfo.Members)} 與按名查成員、名清單。
]
""")]
public interface ITypeInfo{
	[Doc($"""
#Sum[本元資料對應的型別，與官方 {nameof(JsonTypeInfo.Type)} 同義。]

#Descr[
例：從任意一套來源取回 {nameof(JsonTypeInfoSrc)} 所包型別的元資料，
{nameof(Type)} 與 `typeof(...)` 的結果是同一個 {nameof(Type)} 物件，
可用 `Info.{nameof(Type)} == typeof(Xxx)` 直接比較。
]
""")]
	Type Type{get;}

	[Doc($"""
#Sum[型別分類，直接用官方 {nameof(JsonTypeInfoKind)}。]

#Descr[
兩套來源的映射，以及各自會落到哪一檔：
+ `{nameof(str)}`、`i32`、`bool`、{nameof(DateTime)} 這些不可再分、列不出成員的值
	→ {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}，
	官方對標量給出的就是 `{nameof(JsonTypeInfoKind.None)}`；
+ 有成員的物件 → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}；
+ `{nameof(List<int>)}` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Enumerable)}；
+ `{nameof(Dictionary<string, int>)}` → {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Dictionary)}。

例：`{nameof(DateTime)}` 取回 {nameof(JsonTypeInfoKind.None)}（標量）；
`{nameof(DateTime)}[]` 取回 {nameof(JsonTypeInfoKind.Enumerable)}（集合優先於元素型別的標量性）。
]
""")]
	JsonTypeInfoKind Kind{get;}

	[Doc($"""
#Sum[全部成員，順序即契約序：基類在前、同類內按來源的宣告序。]

#Descr[
反射源取收集序；Json 源取官方 {nameof(JsonTypeInfo.Properties)} 的既有序；
契約由 {nameof(TypeInfoSorter)} 統一規整，故兩套來源給出同一個順序。

例：某型別有兩個公開屬性與一個公開字段，反射源交出的順序是
屬性段在前、字段段在後，即兩個屬性按宣告序在前，字段在後；
Json 源交出的 {nameof(JsonTypeInfo.Properties)} 順序與之一致，因而不需要寫兩套適配。

同名遮蔽已去重：
子類用 `new` 遮蔽基類同名成員時，只保留離實例最近的那份宣告，並佔用基類成員原先的位置。
例：基類聲明了 `A`、`B`，子類 `new` 遮蔽了 `A`，
結果是 `A` 仍排第 0 位、`B` 排第 1 位，但第 0 位取到的是子類那份宣告，且 `A` 只出現一次。

名字說明：官方這一側叫 {nameof(JsonTypeInfo.Properties)}（Json）
或 `{nameof(Type)}.{nameof(Type.GetProperties)}` 加 `{nameof(Type.GetFields)}`（反射），
兩者沒有共同名，故本門面自取 {nameof(Members)} 作為統一出口。
]
""")]
	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	//你上面Doc 也沒用nameof語法我想f12跳轉都跳不了
	IReadOnlyList<IMemberInfo> Members{get;}

	[Doc($"""
#Sum[按名（{nameof(IMemberInfo.Name)}）查成員。]

#Descr[
例：`Info.{nameof(TryGetMember)}("Age", out var M)` 命中，`M` 就是 `Age` 的 {nameof(IMemberInfo)}，
且 `M.{nameof(IMemberInfo.CanRead)}` 與 `M.{nameof(IMemberInfo.CanWrite)}` 都是 true；
寫 `"NoSuch"` 則返回 false 且 `M` 為 null。

對照 {nameof(GetMember)}：那個命中不了會拋 {nameof(KeyNotFoundException)}，這個只返回 false。
]
""")]
//TswgTodo 你媽的 能不能好好寫示例??? 說了多少次了 註釋就放一堆乾巴巴的文字?
	bool TryGetMember(str Name, [NotNullWhen(true)] out IMemberInfo? M);

	[Doc($"""
#Sum[按名（{nameof(IMemberInfo.Name)}）取成員，取不到就拋。]

#Descr[
例：`Info.{nameof(GetMember)}("Age")` 返回 `Age` 的 {nameof(IMemberInfo)}；
`Info.{nameof(GetMember)}("NoSuch")` 拋 {nameof(KeyNotFoundException)}，
訊息形如「型別 Xxx 沒有成員 NoSuch。可用成員：Id, Name, Age, ...」，可據此排查拼錯的名字。

對照 {nameof(TryGetMember)}：不確定名字在不在時用那個，只返回 false 不拋異常。
]
""")]
	IMemberInfo GetMember(str Name);

	[Doc($"""
#Sum[可讀成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）。

例：某型別的成員表依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`，
其中 `Secret` 只讀、`Token` 只寫、其餘都可讀可寫，
則 {nameof(ReadableNames)} 是 10 個名，依次跳過只寫的 `Token`：
`["Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Secret", "Level", "Note"]`。

注意判據是「成員能不能讀」，不是「在不在字典視圖的鍵表裏」：
只讀的 `Secret` 在可讀名裏，只寫的 `Token` 不在。

用途：要把物件序列化成一行 SQL 或一份前端表單時，直接遍歷這個清單即可，不必自己過濾成員。
]
""")]
	IReadOnlyCollection<str> ReadableNames{get;}

	[Doc($"""
#Sum[可寫成員名清單，順序同 {nameof(Members)}。]

#Descr[
自研便利（官方無此物）。

例：接上例，{nameof(WritableNames)} 也是 10 個名，依次跳過只讀的 `Secret`：
`["Id", "Name", "Age", "Email", "Married", "Tags", "Extra", "Level", "Token", "Note"]`。

注意 {nameof(ReadableNames)} 與本清單的交集不是全部成員：
只寫的 `Token` 只在可寫名裏，只讀的 `Secret` 只在可讀名裏，兩邊都不含對方獨有的那個。

用途：要按外部字典回填物件時，先拿這個清單擋掉不該寫的鍵（例如外部傳來的 `Secret`）。
]
""")]
	IReadOnlyCollection<str> WritableNames{get;}

	[Doc($"""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.ElementType)} 同義。

例：`{nameof(List<int>)}` 取回 `typeof(i32)`；
`{nameof(Dictionary<string, int>)}` 取回 `typeof(i32)`，
即字典取的是**值**型別，這一點與官方一致；
`i32` 與 `{nameof(DateTime)}` 不是集合，取回 null。
]
""")]
	Type? ElementType{get;}

	[Doc($"""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.KeyType)} 同義。

例：`{nameof(Dictionary<string, int>)}` 取回 `typeof(str)`；
`{nameof(List<int>)}` 不是字典，取回 null；
本屬性與 {nameof(ElementType)} 互斥，非字典型別的 {nameof(KeyType)} 恆為 null，
即使它身上有個 `{nameof(Dictionary<string, int>)}` 型別的成員
（元資料描述的是這個型別本身，不是它的成員型別）。
]
""")]
	Type? KeyType{get;}

	[Doc($"""
#Sum[無參實例工廠，型別與語義就是官方 {nameof(JsonTypeInfo.CreateObject)}。]

#Descr[
null 表示本型別不能建無參實例。反射來源同樣提供這個形狀。

例：有公開無參構造函數的型別，工廠非 null，調一次就得到一個新實例；
只有帶參構造函數的型別、接口、抽象類，工廠是 null；
`i32` 這種值型別反射源給的工廠也是非 null，造的是一個 boxed 的 0。

要用「能不能建」做判斷時，直接查 {nameof(CanMkInst)} 更省事。
]
""")]
	Func<obj>? CreateObject{get;}

	[Doc($"""
#Sum[被包裝的官方 {nameof(JsonTypeInfo)} 本體；反射來源為 null。]

#Descr[
調用方要用官方元資料能力時直接從這裡拿，
例如 {nameof(JsonTypeInfo.Properties)}、`UnmappedMemberHandling`、`NumberHandling`、`PolymorphismOptions`。

例：從 {nameof(JsonTypeInfoSrc)} 取回的元資料，
`Info.{nameof(Json)}!.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;`
可以照官方語義生效；
從 {nameof(ReflTypeInfoSrc)} 取回的同一個型別，`Info.{nameof(Json)}` 是 null，
此時官方能力不可用，只能走本接口自己的成員表。
]
""")]
	JsonTypeInfo? Json{get;}

	[Doc($"""
#Sum[本型別能否建立無參實例。]

#Descr[
自研便利，判據就是官方那條「{nameof(CreateObject)} 是否為 null」。

例：有公開無參構造函數的型別為 true，
只有帶參構造函數的型別、接口、抽象類為 false。

要自己拿工廠時查 {nameof(CreateObject)}；
只是為了判斷要不要走建實例那條路時查本屬性，
免得先取工廠再判空，還得處理可空性警告。
]
""")]
	bool CanMkInst{get;}

	[Doc($"""
#Sum[建立一個無參實例，內部轉調 {nameof(CreateObject)}。]

#Descr[
例：`var U = Info.{nameof(MkInst)}()` 得到一個新實例，
其中各成員是構造函數跑完後的默認態（`i32` 為 0、集合為空集合、`str?` 為 null），
之後可用 `Info.{nameof(GetMember)}("Age")` 配上 {nameof(IMemberInfo.TrySet)} 往裏填值。

不可建時拋 {nameof(NotSupportedException)}（訊息含型別全名）。
先查 {nameof(CanMkInst)} 可以避免這個異常。
]
""")]
	obj? MkInst();
}