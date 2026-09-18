namespace Tsinswreng.CsRefl;

/// 成員排序與去重工具：把兩套來源各自收集到的成員統一成同一份契約序，
/// 並保證同一個成員名只出現一次。
/// 契約：基類在前、同類內保持來源給的相對序（反射源：GetProperties/GetFields 的收集序；
/// Json 源：JsonTypeInfo.Properties 的既有序）。
/// 實現見 TypeInfoSorter.Impl.cs。
internal static partial class TypeInfoSorter{
	/// 規整成員表：先按契約序排序，再按名去重（離實例最近的宣告勝出，
	/// 且佔被遮蔽成員的位置）。
	public static partial IReadOnlyList<IMemberInfo> SortEtDedup(Type Root, IReadOnlyList<IMemberInfo> Members);
}