namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[一個型別的元資料：分類、成員表、實例工廠、集合/字典的鍵值型別。]

#Descr[
兩套來源（反射、JsonTypeInfo）各自實現本接口，對外語義一致。
]

#Descr[
設計原則（與官方 API 的關係）：
官方有的概念一律沿用官方的名字與型別——
`Kind` 就是官方 `JsonTypeInfoKind`，
`ElementType`／`KeyType` 與官方同名，
無參實例工廠就是官方 `JsonTypeInfo.CreateObject`，
Json 來源還可從 `Json` 直接拿到官方 `JsonTypeInfo` 本體。

官方沒有的只有「以字符串為鍵」這一件事：
`Members`／按名查成員／名清單。
]
""")]
public interface ITypeInfo{
	[Doc("""
#Sum[本元資料對應的型別（官方 `JsonTypeInfo.Type` 同義）。]
""")]
	Type Type{get;}

	[Doc("""
#Sum[型別分類，直接用官方 `JsonTypeInfoKind`。]

#Descr[
兩套來源的映射：
標量（字符串/數字/布爾/枚舉/DateTime 等不可再分、不能列成員的值）
→ `JsonTypeInfoKind.None`（官方對標量給出的就是 `None`）；
物件 → `Object`；集合 → `Enumerable`；字典 → `Dictionary`。
]
""")]
	JsonTypeInfoKind Kind{get;}

	[Doc("""
#Sum[全部成員，順序 = 契約序：基類在前、同類內按來源的宣告序。]

#Descr[
反射源取收集序；Json 源取 `Properties` 的既有序。契約由 `TypeInfoSorter` 規整。

同名遮蔽已去重：
子類用 `new` 遮蔽基類同名成員時，
只保留離實例最近的那份宣告，
並佔用基類成員原先的位置——
因此每個 `Name` 只出現一次，非遮蔽成員序不變。

名字說明：官方這一側叫 `Properties`（Json）／
`GetProperties`+`GetFields`（反射），
兩者沒有共同名，
故本門面取 `Members` 作為統一出口（自研的那層）。
]
""")]
	//TswgTodo 爲甚麼用 IReadOnlyList? 這個查詢是O(n)。
	//你上面Doc 也沒用nameof語法我想f12跳轉都跳不了
	IReadOnlyList<IMemberInfo> Members{get;}

	[Doc("""
#Sum[按名（`IMemberInfo.Name`）查成員。]

#Params([[成員名]])

#Rtn[未知返回 false；命中時 `M` 為成員元資料，未命中為 null]
""")]
//TswgTodo你媽的 能不能好好寫示例??? 說了多少次了 註釋就放一堆乾巴巴的文字?
	bool TryGetMember(str Name, [NotNullWhen(true)] out IMemberInfo? M);

	[Doc("""
#Sum[按名（`IMemberInfo.Name`）取成員。]

#Params([[成員名]])

#Rtn[命中的成員元資料]

#Descr[
未知拋 `KeyNotFoundException`，訊息含可用名清單。
]
""")]
	IMemberInfo GetMember(str Name);

	[Doc("""
#Sum[可讀成員名清單，順序同 `Members`。]

#Descr[
自研便利（官方無此物）。
]
""")]
	IReadOnlyCollection<str> ReadableNames{get;}

	[Doc("""
#Sum[可寫成員名清單，順序同 `Members`。]

#Descr[
自研便利（官方無此物）。
]
""")]
	IReadOnlyCollection<str> WritableNames{get;}

	[Doc("""
#Sum[集合的元素型別；非集合為 null。]

#Descr[
與官方 `JsonTypeInfo.ElementType` 同義。
]
""")]
	Type? ElementType{get;}

	[Doc("""
#Sum[字典的鍵型別；非字典為 null。]

#Descr[
與官方 `JsonTypeInfo.KeyType` 同義。
]
""")]
	Type? KeyType{get;}

	[Doc("""
#Sum[無參實例工廠，用的就是官方 `JsonTypeInfo.CreateObject` 的型別與語義。]

#Descr[
null 表示本型別不能建無參實例。反射來源同樣提供這個形狀。
]
""")]
	Func<obj>? CreateObject{get;}

	[Doc("""
#Sum[被包裝的官方 `JsonTypeInfo`；反射來源沒有官方 `JsonTypeInfo`，為 null。]

#Descr[
調用方要用官方元資料能力
（`Properties`／`UnmappedMemberHandling`／
`NumberHandling`／`PolymorphismOptions` 等）時直接從這裡拿。
]
""")]
	JsonTypeInfo? Json{get;}

	[Doc("""
#Sum[本型別能否建立無參實例。]

#Descr[
自研便利：判據就是官方那條「`CreateObject` 是否為 null」。
]
""")]
	bool CanMkInst{get;}

	[Doc("""
#Sum[建立一個無參實例（內部轉調 `CreateObject`）。]

#Rtn[新建的實例]

#Descr[
不可建時拋 `NotSupportedException`。
]
""")]
	obj? MkInst();
}
