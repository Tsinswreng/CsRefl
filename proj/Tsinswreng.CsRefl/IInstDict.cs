namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[一個物件的「淺字典視圖」。]

#Descr[
兩條口徑（職責不同，不可混為一談）：

+ 出現口徑（{nameof(IDictionary<string, object?>)}.{nameof(IDictionary<string, object?>.Keys)}、
	{nameof(IDictionary<string, object?>.Count)}、
	{nameof(IDictionary<string, object?>.Values)}、枚舉）：
	可讀且可寫的成員——這些是視圖「表現為字典」時呈現的鍵，順序 = 成員序；
+ 訪問口徑（索引器、{nameof(IDictionary<string, object?>.TryGetValue)}、
	{nameof(IDictionary<string, object?>.ContainsKey)}）：
	讀寫各按成員自身能力放行，
	只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。
	兩者的差別是故意的：
	字典視圖要能當普通字典改值，
	又不該因為某成員只讀就把它的值藏起來。

例：某實例的成員表裏 `Secret` 只讀、`Token` 只寫，其餘可讀可寫，
則 {nameof(IDictionary<string, object?>.Keys)} 不含 `Secret` 也不含 `Token`，
但 `Dict["Secret"]` 讀得到值、`Dict["Token"] = "x"` 寫得進實例，
這兩個操作都不以「在不在鍵表內」為前提。
]

#Descr[
都不在成員表上的名字才叫「不存在」：
讀拋 {nameof(KeyNotFoundException)}、
{nameof(IDictionary<string, object?>.ContainsKey)} 返回 false。

形狀由型別成員固定，
因而 {nameof(IDictionary<string, object?>.Add)}、
{nameof(IDictionary<string, object?>.Remove)}、
{nameof(IDictionary<string, object?>.Clear)} 拋 {nameof(NotSupportedException)}，
只有改既有成員的值是合法的。
{nameof(IDictionary<string, object?>.Keys)} 是活視圖（與視圖同一份鍵集合），但不提供改形狀的入口。

例：`Dict["Age"] = 31` 合法且會寫回物件；
`Dict["NoSuchKey"] = 1` 拋 {nameof(KeyNotFoundException)}（成員表裏沒這個名字）；
`Dict.Add("NewKey", 1)` 拋 {nameof(NotSupportedException)}（型別上沒有這個成員，加不出來）。
]

#Descr[
用法即 Srefl 時代的 `PropDict`，但修掉了兩個已知缺陷：
+ 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）
+ {nameof(IDictionary<string, object?>.Values)} 不排序
	（舊實現把異質值塞進排序集合，值型別不可互比時會拋）

例：成員序是 `Id`、`Name`、`Age`，
則 {nameof(IDictionary<string, object?>.Keys)} 就是這個順序，
可以直接拿去當 SQL 的列序或前端表格的欄位序。
]
""")]
//TswgTodo 是不是有點違反里氏替換了?
//我拿到一個IDict 我不知道裏面是甚麼實現, 我對他增減鍵 卻會報錯。
//能不能仿照JsonNodeDict的思路? 或者分兩種InstDict ,
//一種是 不讓增減鍵的,
//一種是能增減鍵的。不讓增減鍵的容易做就先留做好的一版。
public interface IInstDict:IDictionary<str,obj?>{
	[Doc($"""
#Sum[視圖背後的物件。]

#Descr[
例：`new {nameof(InstDict)}(User, Info).{nameof(Target)}` 就是傳進去的那個 `User`，
讀寫都是往這個物件上落，
故視圖本身不持有成員值的副本，改一次立刻能從原物件看到。
]
""")]
	obj? Target{get;}

	[Doc($"""
#Sum[視圖所用到的型別元資料。]

#Descr[
鍵表、讀寫能力都從這份元資料現算，故視圖的形狀由它決定。

例：拿 {nameof(ITypeInfo.Type)} 不同的元資料來建視圖，
鍵集合與可寫成員都會跟著變；
用基類的元資料建視圖時，只有基類宣告的成員在視圖裏。
]
""")]
	ITypeInfo TypeInfo{get;}
}