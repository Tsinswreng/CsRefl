namespace Tsinswreng.CsRefl;

/// 型別元資料的分類。
/// 對應「動態對象操作」所需的型別形狀，也直接對應 JsonTypeInfo.Kind：
/// 反射來源靠介面分析得出同一結論，JsonTypeInfo 來源直接映射 Kind。
public enum ETypeKind{
	/// 標量：字符串、數字、布爾、枚舉、DateTime 等不可再分、不能列成員的值。
	Scalar,
	/// 物件：可列成員並逐個讀寫的類或結構。
	Object,
	/// 集合：元素型別單一的可枚舉型別（如 List&lt;T&gt;、數組）。
	Enumerable,
	/// 字典：鍵值成對的可枚舉型別（如 Dictionary&lt;K,V&gt;）。
	Dictionary,
}