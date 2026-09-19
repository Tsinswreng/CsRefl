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
{nameof(InstDict)} 的鍵序與 CsSql 的列序都直接吃這個順序，
故兩套來源切換時下游不必改代碼。

實現見 `TypeInfoSorter.Impl.cs`。
]
""")]
internal static partial class TypeInfoSorter{
	[Doc($"""
#Sum[規整成員表：先按契約序排序，再按名去重。]

#Params([[Root, 成員表所屬的型別，即實例的型別], [Members, 待規整的成員表]])

#Rtn[規整後的只讀成員表]

#Descr[
去重時離實例最近的宣告勝出，且佔被遮蔽成員的位置。

實測：`PoUser` 這條繼承鏈上基類 `PoUserBase` 宣告 `Id`、`Name`，
子類宣告其餘成員，故輸出前兩位是基類那兩個；
另一條鏈上子類用 `new` 再宣告 `Id`，輸出是 `Name`、`Id`、`Age` 三項，
`Id` 只出現一次（位置仍是第 2 位），且按名查到的那份其宣告型別是子類。

實現是防禦性的：
現行兩套來源實測都不產生重複名（{nameof(Type)} 的收集方法自帶隱藏語義、
官方 {nameof(System.Text.Json.Serialization.Metadata.JsonTypeInfo.Properties)} 也不含重複名），
但門面對外承諾「成員名唯一」，故不依賴來源剛好守規矩。
]
""")]
	public static partial IReadOnlyList<IMemberInfo> SortEtDedup(Type Root, IReadOnlyList<IMemberInfo> Members);

	// ---- 私有輔助（實現見 TypeInfoSorter.Impl.cs）----

	[Doc($"""
#Sum[`Declaring` 相對於 `Root` 的繼承深度（`Root` 自身為 0）。]

#Params([[Root, 成員表所屬的型別], [Declaring, 成員的宣告型別]])

#Rtn[繼承深度]

#Descr[
聲明型別若不是 `Root` 本身也不是它的基類
（例如手工註冊表塞入的畸形元資料、或成員聲明在接口上），
繼承鏈走不到 `Root`，此時返回整條鏈的長度——
順序因此不可靠，但不會死循環
（接口與根型別的 {nameof(Type.BaseType)} 為 null，走一步即退出）。

實測：查 `PoUser` 時，基類 `PoUserBase` 宣告的 `Id` 與 `Name` 得 1，
`PoUser` 自己宣告的 `Age` 得 0，故降序排列後 `Id`、`Name` 排在 `Age` 之前；
直接以 `PoUserBase` 為 `Root` 時，`Id` 得 0。
]
""")]
	private static partial int DepthOf(Type Root, Type Declaring);
}