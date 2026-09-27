namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[成員排序與去重工具：把兩套來源各自收集到的成員統一成同一份契約序。]

#Descr[
契約：基類在前、同類內保持來源給的相對序
（反射源：{nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)} 的收集序；
Json 源：{nameof(System.Text.Json.Serialization.Metadata.JsonTypeInfo.Properties)} 的既有序）。

並保證同一個成員名只出現一次。

實測：`PoUser` 的成員表經本工具規整後是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`，
`Id` 與 `Name` 是基類 `PoUserBase` 宣告的（故排在前兩個位置）；
{nameof(InstViewDict)} 的鍵序與 CsSql 的列序都直接吃這個順序，
故兩套來源切換時下游不必改代碼。

實現見 `TypeInfoSorter.Impl.cs`。
]
""")]
internal static partial class TypeInfoSorter{
	[Doc($$"""
#Sum[規整成員表：先按契約序排序，再按名去重。]

#Params([[Root, 成員表所屬的型別，即實例的型別], [Members, 待規整的成員表]])

#Rtn[規整後的只讀成員表]

#Descr[
本包內部在建構子裏這樣調（不在對外門面上）：

```csharp
// 兩條來源的建構子內部：
var Sorted = TypeInfoSorter.SortEtDedup(typeof(PoUser), Collected);
// Sorted 依次是 Id、Name、Age、…、Note：基類 PoUserBase 宣告的 Id、Name 排在最前。
```

去重時離實例最近的宣告勝出，且佔被遮蔽成員的位置。
]
""")]
	//TswgNote 看不懂
	public static partial IReadOnlyList<IMemberInfo> SortEtDedup(Type Root, IReadOnlyList<IMemberInfo> Members);

	// ---- 私有輔助（實現見 TypeInfoSorter.Impl.cs）----

	[Doc($"""
#Sum[`Declaring` 相對於 `Root` 的繼承深度（`Root` 自身為 0）。]

#Params([[Root, 成員表所屬的型別], [Declaring, 成員的宣告型別]])

#Rtn[繼承深度]

#Descr[
實測：查 `PoUser` 時，基類 `PoUserBase` 宣告的 `Id` 與 `Name` 得 1，
`PoUser` 自己宣告的 `Age` 得 0，故降序排列後 `Id`、`Name` 排在 `Age` 之前。
]
""")]
	private static partial int DepthOf(Type Root, Type Declaring);
}




