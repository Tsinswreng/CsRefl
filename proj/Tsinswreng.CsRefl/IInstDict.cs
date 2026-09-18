namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc("""
#Sum[一個物件的「淺字典視圖」。]

#Descr[
口徑（兩條，職責不同，不可混為一談）：

+ 出現口徑（`Keys` / `Count` / `Values` / 枚舉）：
	可讀且可寫的成員——這些是視圖「表現為字典」時呈現的鍵，
	順序 = 成員序；
+ 訪問口徑（索引器 / `TryGetValue` / `ContainsKey`）：
	讀寫各按成員自身能力放行，
	只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。
	兩者的差別是故意的：
	字典視圖要能當普通字典改值，
	又不該因為某成員只讀就把它的值藏起來。
]

#Descr[
都不在成員表上的名字才叫「不存在」：
讀拋 `KeyNotFoundException`、`ContainsKey` 返回 false。

形狀由型別成員固定，
因而 `Add` / `Remove` / `Clear` 拋 `NotSupportedException`，
只有改既有成員的值是合法的。
`Keys` 是活視圖（與視圖同一份鍵集合），但不提供改形狀的入口。
]

#Descr[
用法即 Srefl 時代的 `PropDict`，但修掉了兩個已知缺陷：
+ 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）
+ `Values` 不排序（舊實現把異質值塞進排序集合，值型別不可互比時會拋）
]
""")]
public interface IInstDict:IDictionary<str,obj?>{
	[Doc("""
#Sum[視圖背後的物件。]
""")]
	obj? Target{get;}

	[Doc("""
#Sum[視圖所用到的型別元資料。]
""")]
	ITypeInfo TypeInfo{get;}
}