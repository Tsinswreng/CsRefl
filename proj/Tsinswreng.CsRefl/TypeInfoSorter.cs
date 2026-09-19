namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[成員排序與去重工具：把兩套來源各自收集到的成員統一成同一份契約序。]

#Descr[
契約：基類在前、同類內保持來源給的相對序
（反射源：{nameof(Type)}.{nameof(Type.GetProperties)} 與 {nameof(Type)}.{nameof(Type.GetFields)} 的收集序；
Json 源：{nameof(System.Text.Json.Serialization.Metadata.JsonTypeInfo.Properties)} 的既有序）。

並保證同一個成員名只出現一次。

例：`{nameof(InstDict)}` 的鍵序、CsSql 的列序都依賴這份契約序，
故兩套來源切換時下游不必改代碼。

實現見 `TypeInfoSorter.Impl.cs`。
]
""")]
internal static partial class TypeInfoSorter{
	[Doc($"""
#Sum[規整成員表：先按契約序排序，再按名去重。]

#Params([[成員表所屬的型別，即實例的型別], [待規整的成員表]])

#Rtn[規整後的只讀成員表]

#Descr[
去重時離實例最近的宣告勝出，且佔被遮蔽成員的位置。

例：繼承鏈上 `Base` 先、`Derived` 後，故基類成員排前面；
若子類用 `new` 遮蔽了基類的某個成員，輸出裏仍只有一個該名字的成員，
位置不變（沿用被遮蔽那份的位置），但取到的是子類那份宣告。

實現是防禦性的：
現行兩套來源實測都不產生重複名（{nameof(Type)} 的收集方法自帶隱藏語義、
官方 {nameof(System.Text.Json.Serialization.Metadata.JsonTypeInfo.Properties)} 也不含重複名），
但門面對外承諾「成員名唯一」，故不依賴來源剛好守規矩。
]
""")]
	public static partial IReadOnlyList<IMemberInfo> SortEtDedup(Type Root, IReadOnlyList<IMemberInfo> Members);
}