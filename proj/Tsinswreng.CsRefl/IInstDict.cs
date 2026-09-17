namespace Tsinswreng.CsRefl;

/// 一個物件的「淺字典視圖」：鍵 = 可讀且可寫的成員（按成員序），
/// 讀 = 從物件讀，寫 = 寫回物件。字典形狀由型別成員固定，
/// 因而 Add / Remove / Clear 拋 NotSupportedException，只有改既有鍵的值是合法的。
///
/// 用法即 Srefl 時代的 PropDict，但修掉了兩個已知缺陷：
/// - 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）；
/// - Values 不排序（舊實現把異質值塞進排序集合，值型別不可互比時會拋）。
public interface IInstDict:IDictionary<str,obj?>{
	/// 視圖背後的物件。
	obj? Target{get;}
	/// 視圖所用到的型別元資料。
	ITypeInfo TypeInfo{get;}
}