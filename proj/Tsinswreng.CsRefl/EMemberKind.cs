namespace Tsinswreng.CsRefl;

/// 成員元資料的分類：屬性還是字段。
/// 這是標準反射裏 PropertyInfo/FieldInfo 的區分；門面把兩者統一為 IMemberInfo，
/// 用本枚舉保留「種類」這個維度。JsonTypeInfo 來源目前只會給出 Property
/// （非泛型 JsonPropertyInfo 不暴露 IsProperty，見 JsonMemberInfo 的說明）。
public enum EMemberKind{
	/// 屬性（property）。
	Property,
	/// 字段（field）。
	Field,
}