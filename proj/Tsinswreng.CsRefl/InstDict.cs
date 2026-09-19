namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[一個物件的「淺字典視圖」。]

#Descr[
兩條口徑（職責不同，不可混為一談）：

+ 出現口徑（{nameof(Keys)}、{nameof(Count)}、{nameof(Values)}、枚舉）：
	{nameof(IMemberInfo.CanRead)} 與 {nameof(IMemberInfo.CanWrite)} 都為 true 的成員，
	順序 = 成員序；
+ 訪問口徑（索引器、{nameof(TryGetValue)}、{nameof(ContainsKey)}）：
	讀只要求可讀、寫只要求可寫，判據都是成員表而不是鍵表。

實測（`PoUser`，成員序為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`，
其中 `Secret` 只讀、`Token` 只寫）：

+ {nameof(Count)} 是 9，
	{nameof(Keys)} 依次為 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Note`
	（少了只讀的 `Secret` 與只寫的 `Token`）；
+ `Dict["Secret"]` 讀得到 "s"（只讀成員讀得，只是不是鍵）；
+ `Dict["Token"] = "t1"` 寫進物件（只寫成員寫得進，但 `Dict["Token"]` 讀會拋）；
+ `Dict.{nameof(ContainsKey)}("Secret")` 是 false，而 `Dict["Secret"]` 成功。
]

#Descr[
都不在成員表上的名字才叫「不存在」：
讀拋 {nameof(KeyNotFoundException)}、{nameof(ContainsKey)} 返回 false。

形狀由型別成員固定，
因而 {nameof(Add)}、{nameof(Remove)}、{nameof(Clear)} 拋 {nameof(NotSupportedException)}，
只有改既有成員的值是合法的。

實測：`Dict["NoSuch"]` 與 `Dict["NoSuch"] = 1` 都拋 {nameof(KeyNotFoundException)}，
訊息含可用鍵清單（實測含 "Level" 這個字串可被斷言）；
`Dict["Secret"] = "x"` 拋 {nameof(InvalidOperationException)}，訊息指名 "Secret"
（成員在、但不可寫，與「鍵不存在」是兩種錯）；
`Dict.{nameof(Add)}("NewKey", 1)`、`Dict.{nameof(Remove)}("Age")`、`Dict.{nameof(Clear)}()`
三者都拋 {nameof(NotSupportedException)}。
]

#Descr[
用法即 Srefl 時代的 `PropDict`，但修掉了兩個已知缺陷：
+ 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）
+ {nameof(Values)} 不排序（舊實現把異質值塞進排序集合，值型別不可互比時會拋）

實測：上例的鍵序就是成員序（`Id` 在 `Note` 之前，而不是字母序的 `Age` 在前），
故可直接拿去當 SQL 的列序或前端表格的欄位序。
]

#Descr[
建構子與 {nameof(IDictionary<string, object?>)} 成員的實現見 `InstDict.Impl.cs`。
]
""")]
public partial class InstDict:IInstDict{
	[Doc($"""
#Sum[視圖背後的物件；與官方 `JsonObject` 那種「本體直接公開」同款寫法。]

#Descr[
實測：`Dict["Level"] = 8` 之後 `User.Level` 就是 8，
讀寫都落在這個物件上，視圖本身不持有成員值的副本。
]
""")]
	private readonly obj _target;

	[Doc($"""
#Sum[視圖所用到的型別元資料。]

#Descr[
實測：其 {nameof(ITypeInfo.Type)} 是 `typeof(PoUser)`；
鍵表與可讀可寫判據都從它現算，故換一份元資料建視圖，鍵集合也跟著變。
]
""")]
	private readonly ITypeInfo _typeInfo;

	[Doc($"""
#Sum[出現口徑的鍵清單：可讀且可寫的成員名，按成員序。]

#Descr[
與 SQL 列序、前端欄位序保持一致；
修掉舊 `PropDict` 用排序集合把順序改成字母序的問題。

實測：`PoUser` 的這份清單是 9 個名，依次為
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Note`；
只讀的 `Secret` 與只寫的 `Token` 都不在裏面。
]
""")]
	private readonly List<str> _keys;

	[Doc($"""
#Sum[{nameof(Keys)} 對外只讀包裝：活視圖（與 {nameof(_keys)} 同一份數據，不複製），但不許外部改形狀。]

#Descr[
實測：對外只暴露 {nameof(ICollection<string>)}，
外部拿到的 {nameof(Keys)} 沒有增刪入口，故繞不過「形狀由型別成員固定」這條約束。
]
""")]
	private readonly ICollection<str> _keysView;

	[Doc($"""
#Sum[讀寫各按成員能力放行。]

#Descr[
讀要求可讀、寫要求可寫，兩者的判據都是成員表而非 {nameof(_keys)}。
實現見 `InstDict.Impl.cs`。

實測（`PoUser`，`Age` 起初 26、`Secret` 初值 "s"）：

+ `Dict["Age"]` 讀到 26（boxed 的 `i32`）；
+ `Dict["Secret"]` 讀到 "s"（只讀成員不在鍵表內但可讀）；
+ `Dict["Token"] = "t1"` 寫進物件（只寫成員讀不到但可寫）；
+ `Dict["NoSuch"]` 與 `Dict["NoSuch"] = 1` 都拋 {nameof(KeyNotFoundException)}。
]
""")]
	public partial obj? this[str Key]{
		get;
		set;
	}

	[Doc($"""
#Sum[視圖背後的物件。]

#See[{nameof(IInstDict.Target)}]
""")]
	public obj? Target{
		get{
			return _target;
		}
	}

	[Doc($"""
#Sum[視圖所用到的型別元資料。]

#See[{nameof(IInstDict.TypeInfo)}]
""")]
	public ITypeInfo TypeInfo{
		get{
			return _typeInfo;
		}
	}

	[Doc($"""
#Sum[視圖可被寫入既有成員，故不是只讀字典。]

#Descr[
{nameof(IsReadOnly)} 的語義見 {nameof(IDictionary<string, object?>)}。

實測：這個屬性恆為 false（視圖的核心用途就是改既有成員的值，下游看它就敢改），
但「能改值」不等於「能增刪鍵」，增刪仍拋 {nameof(NotSupportedException)}。
]
""")]
	public bool IsReadOnly{
		get{
			return false;
		}
	}

	[Doc($"""
#Sum[出現口徑的鍵數 = 可讀可寫成員數。]
#Descr[
實測：`PoUser` 的可讀可寫成員是 9 個，故這裡是 9；
只讀的 `Secret` 與只寫的 `Token` 都不計入。
]
""")]
	public int Count{
		get{
			return _keys.Count;
		}
	}

	[Doc($"""
#Sum[出現口徑的鍵集合。]
#Descr[
活視圖，與視圖同一份鍵集合，但不提供改形狀的入口。

實測：`foreach(var K in Dict.Keys)` 依次拿到 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Note`；
這個順序與名前綴的成員序一致，可直接當列序用。
]
""")]
	public ICollection<str> Keys{
		get{
			return _keysView;
		}
	}

	[Doc($"""
#Sum[出現口徑的值集合。]

#Descr[
每次訪問現算（按出現口徑的鍵序取一遍），
不做排序（修掉舊 `PropDict` 對異質值排序會拋的問題）。

實測：`PoUser` 實例的值依次是 boxed 的 `i64` 1、`str` "小明"、boxed 的 `i32` 26、
null（`Email` 未賦值）、false、空列表、空字典、boxed 的 `i32` 0、`str` "n"；
`i64`、`str`、集合混在一起照樣平鋪返回，不會因為異質值不可互比而拋
（舊 `PropDict` 正是在這裡拋）；
代價是每次訪問都重新讀一遍物件的值。
]
""")]
	public ICollection<obj?> Values{
		get{
			return BuildValues();
		}
	}

	[Doc($"""
#Sum[鍵是否存在於出現口徑（= 可讀且可寫的成員）。]

#Descr[
實測：`Dict.{nameof(ContainsKey)}("Age")` 是 true；
`Dict.{nameof(ContainsKey)}("NoSuch")` 是 false；
`Dict.{nameof(ContainsKey)}("Secret")` 也是 false（只讀成員不在鍵表內），
但同一時刻 `Dict["Secret"]` 讀得到值，兩者判據不同。
]

#See[{nameof(IInstDict.ContainsKey)}]
""")]
	public partial bool ContainsKey(str Key);

	[Doc($"""
#Sum[按成員表取值：成員存在且可讀即成功；未知成員返回 false。]

#Descr[
實測：`Dict.{nameof(TryGetValue)}("Age", out var V)` 返回 true，V 是 boxed 的 `i32` 26；
`Dict.{nameof(TryGetValue)}("Secret", out var S)` 返回 true（只讀成員取得到）；
`Dict.{nameof(TryGetValue)}("Token", out _)` 返回 false（只寫成員取不到）；
`Dict.{nameof(TryGetValue)}("NoSuch", out _)` 返回 false（未知鍵）。
]

#See[{nameof(IInstDict.TryGetValue)}]
""")]
	public partial bool TryGetValue(str Key, out obj? Value);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#Descr[
實測：`Dict.{nameof(Add)}("NewKey", 1)` 恆拋；
型別上沒有這個成員，字典視圖加不出來。
]

#See[{nameof(IInstDict.Add)}]
""")]
	public partial void Add(str Key, obj? Value);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#See[{nameof(IInstDict.Add)}]
""")]
	public partial void Add(KeyValuePair<str, obj?> Item);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#Descr[
實測：`Dict.{nameof(Remove)}("Age")` 恆拋；
成員在運行期無法從型別上抹掉，這個操作沒有可兌現的語義。
]

#See[{nameof(IInstDict.Remove)}]
""")]
	public partial bool Remove(str Key);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#See[{nameof(IInstDict.Remove)}]
""")]
	public partial bool Remove(KeyValuePair<str, obj?> Item);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#Descr[
實測：`Dict.{nameof(Clear)}()` 恆拋；
真要「全部歸零」得逐個成員改值，形狀本身清不掉。
]

#See[{nameof(IInstDict.Clear)}]
""")]
	public partial void Clear();

	[Doc($"""
#Sum[鍵值對是否都在視圖內且相等。]

#Descr[
實測：`Dict.{nameof(Contains)}` 傳入鍵 `Age`、值 boxed 的 `i32` 26 時為 true；
值傳 31（與物件不一致）為 false；
鍵傳 `Token`（只寫，取不到值）也為 false。
]

#See[{nameof(IInstDict.Contains)}]
""")]
	public partial bool Contains(KeyValuePair<str, obj?> Item);

	[Doc($"""
#Sum[按出現口徑的鍵序拷貝鍵值對到數組。]

#Descr[
下標為負或容量不足時拋 {nameof(ArgumentException)} 系列。

實測：`{nameof(CopyTo)}(Buf, 2)` 時 `Buf` 的下標 0 與 1 保持原樣，
自下標 2 起依次寫入 `Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Level`、`Note`；
`Buf` 長度只有 3 時拋 {nameof(ArgumentException)}
（修前裸拋 `IndexOutOfRangeException`，對調用方毫無線索）。
]

#See[{nameof(IInstDict.CopyTo)}]
""")]
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex);

	[Doc($"""
#Sum[按出現口徑的鍵序逐項取值的迭代器。]

#Descr[
實測：`foreach(var Kv in Dict)` 依次拿到 9 對，
第一對是 `Id` 配 boxed 的 `i64` 1，第七對的鍵是 `Extra`，最後一對的鍵是 `Note`；
每次迭代都現讀物件的值。
]

#See[{nameof(IInstDict.GetEnumerator)}]
""")]
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator();

	// ---- 私有輔助（實現見 InstDict.Impl.cs）----

	[Doc($"""
#Sum[按鍵讀出成員值。]

#Params([[Key, 要讀的鍵]])

#Rtn[讀出的值]

#Descr[
成員不存在或不可讀時拋 {nameof(KeyNotFoundException)}，
訊息含出現口徑的鍵清單。

實測：`{nameof(ReadCell)}("Age")` 讀出 boxed 的 `i32` 26；
`{nameof(ReadCell)}("Secret")` 讀出 "s"（只讀成員也讀得，判據是成員表而非鍵表）；
`{nameof(ReadCell)}("Token")` 拋 {nameof(KeyNotFoundException)}（只寫成員不可讀）。
]
""")]
	private partial obj? ReadCell(str Key);

	[Doc($"""
#Sum[按鍵寫入成員值。]

#Params([[Key, 要寫的鍵], [Value, 要寫入的值]])

#Descr[
成員不存在拋 {nameof(KeyNotFoundException)}；
成員存在但不可寫、或值型別不符，拋 {nameof(InvalidOperationException)}。

實測：`{nameof(WriteCell)}("Age", 31)` 之後物件上的 `Age` 是 31；
`{nameof(WriteCell)}("Secret", "x")` 拋 {nameof(InvalidOperationException)}（只讀）；
`{nameof(WriteCell)}("NoSuch", 1)` 拋 {nameof(KeyNotFoundException)}（不存在），兩種錯分得開。
]
""")]
	private partial void WriteCell(str Key, obj? Value);

	[Doc($"""
#Sum[按出現口徑的鍵序逐鍵取值。]

#Rtn[值集合，順序同字段 {nameof(_keys)}]

#Descr[
逐鍵求值，不做排序，故異質值不會因不可互比而拋。

實測：`PoUser` 實例得到 9 個值，依次為 boxed 的 `i64` 1、`str` "小明"、
boxed 的 `i32` 26、null、false、空列表、空字典、boxed 的 `i32` 0、`str` "n"。
]
""")]
	private partial ICollection<obj?> BuildValues();

	// 顯式接口實現（IEnumerable.GetEnumerator）不能標 partial，故不在聲明側列出；
	// 它只是對上面公開 GetEnumerator 的一行轉發，見 InstDict.Impl.cs。

	[Doc($"""
#Sum[用一個實例與它的型別元資料建視圖。]

#Params([[Target, 視圖背後的物件，不允許 null], [TypeInfo, 該物件的型別元資料]])

#Descr[
{nameof(Target)} 不允許 null（視圖必須能讀寫實例）。

實測：`new {nameof(InstDict)}(User, Info)` 之後
`Dict["Level"] = 8` 使 `User.Level` 變成 8；
`Target` 傳 null 或 `TypeInfo` 傳 null 都在構造期拋 {nameof(ArgumentNullException)}，
不留到讀寫時才暴露。
]
""")]
	public partial InstDict(obj Target, ITypeInfo TypeInfo);
}