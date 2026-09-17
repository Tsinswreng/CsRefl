namespace Tsinswreng.CsRefl;

/// 成員排序工具：把兩套來源各自收集到的成員統一成同一份契約順序。
/// 契約：基類在前、同類內保持來源給的相對序（反射源：元數據表序；
/// Json 源：JsonPropertyInfo.Properties 的既有序）。
/// Sort 的實現見 TypeInfoSorter.Impl.cs。
internal static partial class TypeInfoSorter{
}