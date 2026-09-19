namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[一個物件的「淺字典視圖」。]

#Descr[
兩條口徑（職責不同，不可混為一談）：

+ 出現口徑（{nameof(IDictionary<string, object?>.Keys)}、
	{nameof(IDictionary<string, object?>.Count)}、
	{nameof(IDictionary<string, object?>.Values)}、枚舉）：
	可讀且可寫的成員，順序 = 成員序；
+ 訪問口徑（索引器、{nameof(IDictionary<string, object?>.TryGetValue)}、
	{nameof(IDictionary<string, object?>.ContainsKey)}）：
	讀只要求可讀、寫只要求可寫，判據都是成員表而不是鍵表。

實測（`PoUser`，成員序 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、
`Secret`、`Level`、`Token`、`Note`，其中 `Secret` 只讀、`Token` 只寫）：

+ {nameof(IDictionary<string, object?>.Count)} 是 9，
	{nameof(IDictionary<string, object?>.Keys)} 依次為
	`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Note`
	（不含只讀的 `Secret`，也不含只寫的 `Token`）；
+ `Dict["Secret"]` 讀得到 "s"（只讀成員讀得，只是不是鍵）；
+ `Dict["Token"] = "t1"` 寫進物件（只寫成員寫得進，但 `Dict["Token"]` 讀會拋）；
+ `Dict.{nameof(IDictionary<string, object?>.ContainsKey)}("Secret")` 是 false，
	而同一時刻 `Dict["Secret"]` 成功，兩者判據不同。
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

實測：`Dict["NoSuch"]` 與 `Dict["NoSuch"] = 1` 都拋 {nameof(KeyNotFoundException)}，
訊息含可用鍵清單（實測含 `"Level"` 這個子串）；
`Dict["Secret"] = "x"` 拋 {nameof(InvalidOperationException)}（成員在、但不可寫）；
`Dict.Add("NewKey", 1)`、`Dict.Remove("Age")`、`Dict.Clear()` 都拋 {nameof(NotSupportedException)}。
]

#Descr[
用法即 Srefl 時代的 `PropDict`，但修掉了兩個已知缺陷：
+ 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）
+ {nameof(IDictionary<string, object?>.Values)} 不排序
	（舊實現把異質值塞進排序集合，值型別不可互比時會拋）

實測：上例的鍵序就是成員序（`Id` 在 `Note` 之前，而不是字母序的 `Age` 在前），
值裏 `i64`、`str`、`i32`、集合混在一起也照樣平鋪返回，不會拋。
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
實測：`Dict["Level"] = 8` 之後原物件的 `Level` 就是 8，
讀寫都落在這個物件上，視圖本身不持有成員值的副本。
]
""")]
	obj? Target{get;}

	[Doc($"""
#Sum[視圖所用到的型別元資料。]

#Descr[
鍵表與讀寫能力都從這份元資料現算，故視圖的形狀由它決定。

實測：用 `PoUser` 的元資料建的視圖，其 {nameof(ITypeInfo.Type)} 是 `typeof(PoUser)`；
換用基類 `PoUserBase` 的元資料建視圖，鍵就只有 `Id` 與 `Name` 兩個。
]
""")]
	ITypeInfo TypeInfo{get;}
}