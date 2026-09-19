namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[一個物件的「淺字典視圖」。]

#Descr[
口徑（兩條，職責不同，不可混為一談）：

+ 出現口徑（{nameof(Keys)} / {nameof(Count)} / {nameof(Values)} / 枚舉）：
	可讀且可寫的成員——這些是視圖「表現為字典」時呈現的鍵，
	順序 = 成員序；
+ 訪問口徑（索引器 / {nameof(TryGetValue)} / {nameof(ContainsKey)}）：
	讀寫各按成員自身能力放行，
	只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。
	兩者的差別是故意的：
	字典視圖要能當普通字典改值，
	又不該因為某成員只讀就把它的值藏起來。

例：成員表裏 `Secret` 只讀、`Token` 只寫、其餘可讀可寫，
則 {nameof(Keys)} 既不含 `Secret` 也不含 `Token`，
但 `Dict["Secret"]` 讀得到值、`Dict["Token"] = "x"` 寫得進實例，
這兩個操作都不以「在不在鍵表內」為前提。
]

#Descr[
都不在成員表上的名字才叫「不存在」：
讀拋 {nameof(KeyNotFoundException)}、{nameof(ContainsKey)} 返回 false。

形狀由型別成員固定，
因而 {nameof(Add)} / {nameof(Remove)} / {nameof(Clear)} 拋 {nameof(NotSupportedException)}，
只有改既有成員的值是合法的。

例：`Dict["Age"] = 31` 合法且會寫回物件；
`Dict["NoSuchKey"] = 1` 拋 {nameof(KeyNotFoundException)}（成員表裏沒這個名字）；
`Dict.{nameof(Add)}("NewKey", 1)` 拋 {nameof(NotSupportedException)}（型別上沒有這個成員，加不出來）。
]

#Descr[
用法即 Srefl 時代的 `PropDict`，但修掉了兩個已知缺陷：
+ 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）
+ {nameof(Values)} 不排序（舊實現把異質值塞進排序集合，值型別不可互比時會拋）

例：成員序是 `Id`、`Name`、`Age`，
則 {nameof(Keys)} 就是這個順序，可直接拿去當 SQL 的列序或前端表格的欄位序。
]

#Descr[
建構子與 {nameof(IDictionary<string, object?>)} 成員的實現見 `InstDict.Impl.cs`。
]
""")]
public partial class InstDict:IInstDict{
	[Doc($"""
#Sum[視圖背後的物件。]

#Descr[
例：`new {nameof(InstDict)}(User, Info).{nameof(Target)}` 就是傳進去的 `User`，
讀寫都落在這個物件上，視圖本身不持有成員值的副本。
]
""")]
	private readonly obj _target;

	[Doc($"""
#Sum[視圖所用到的型別元資料。]

#Descr[
例：鍵表、可讀可寫判據都從這份元資料現算，
故換一份元資料建視圖，鍵集合也會跟著變。
]
""")]
	private readonly ITypeInfo _typeInfo;

	[Doc($"""
#Sum[出現口徑的鍵清單：可讀且可寫的成員名，按成員序。]

#Descr[
與 SQL 列序、前端欄位序保持一致；
修掉舊 `PropDict` 用排序集合把順序改成字母序的問題。

例：成員序是 `Id`、`Name`、`Age` 時，這個清單就是這三個名；
只讀成員與只寫成員都不進來。
]
""")]
	private readonly List<str> _keys;

	[Doc($"""
#Sum[{nameof(Keys)} 對外只讀包裝：活視圖（與 {nameof(_keys)} 同一份數據，不複製），但不許外部改形狀。]

#Descr[
例：外部拿到它之後 {nameof(ICollection<string>)}.{nameof(ICollection<string>.Add)} 會拋，
故繞不過「形狀由型別成員固定」這條約束（早期版本裸露內部清單時正是從這裡被繞過的）。
]
""")]
	private readonly ICollection<str> _keysView;

	[Doc($"""
#Sum[讀寫各按成員能力放行。]

#Descr[
讀要求可讀、寫要求可寫，兩者的判據都是成員表而非 {nameof(_keys)}。
實現見 `InstDict.Impl.cs`。

例：`Dict["Secret"]` 讀得到（只讀成員不在鍵表內但可讀）；
`Dict["Token"] = "x"` 寫得進（只寫成員讀不到但可寫）；
`Dict["NoSuch"]` 拋 {nameof(KeyNotFoundException)}。
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

例：下游拿到 {nameof(IDictionary<string, object?>)} 後會看這個屬性決定能不能改值，
故這裡必須是 false（視圖的核心用途就是改既有成員的值）；
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
例：成員表裏有 10 個可讀可寫成員時這裡就是 10，
只讀成員與只寫成員都不計入。
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

例：`foreach(var K in Dict.{nameof(Keys)})` 拿到的就是成員序的鍵；
想按這個順序取一遍值，用 {nameof(Values)} 或直接枚舉視圖即可。
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

例：成員值是 `i32`、`str`、集合混在一起時，
這裡照成員序平鋪返回，不會因為異質值不可互比而拋；
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
例：`Dict.{nameof(ContainsKey)}("Age")` 為 true；
`Dict.{nameof(ContainsKey)}("Secret")` 為 false（只讀，不在鍵表內）。
]

#See[{nameof(IInstDict.ContainsKey)}]
""")]
	public partial bool ContainsKey(str Key);

	[Doc($"""
#Sum[按成員表取值：成員存在且可讀即成功；未知成員返回 false。]

#Descr[
例：`Dict.{nameof(TryGetValue)}("Secret", out var V)` 返回 true（只讀成員取得到）；
取只寫成員返回 false。
]

#See[{nameof(IInstDict.TryGetValue)}]
""")]
	public partial bool TryGetValue(str Key, out obj? Value);

	[Doc($"""
#Sum[形狀由型別成員固定，本操作恆拋 {nameof(NotSupportedException)}。]

#Descr[
例：`Dict.{nameof(Add)}("NewKey", 1)` 恆拋；
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
例：`Dict.{nameof(Remove)}("Age")` 恆拋；
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
例：`Dict.{nameof(Clear)}()` 恆拋；
真要「全部歸零」得逐個成員改值，形狀本身清不掉。
]

#See[{nameof(IInstDict.Clear)}]
""")]
	public partial void Clear();

	[Doc($"""
#Sum[鍵值對是否都在視圖內且相等。]

#Descr[
例：`Dict.{nameof(Contains)}(Pair)` 在鍵可讀且值相等時為 true；
鍵是只寫成員或值不同則為 false。
]

#See[{nameof(IInstDict.Contains)}]
""")]
	public partial bool Contains(KeyValuePair<str, obj?> Item);

	[Doc($"""
#Sum[按出現口徑的鍵序拷貝鍵值對到數組。]

#Descr[
下標為負或容量不足時拋 {nameof(ArgumentException)} 系列。

例：`Dict.{nameof(CopyTo)}(Buf, 0)` 把鍵值對按成員序填進 `Buf`；
`Buf` 太短時訊息會說明需要幾個位置、實際剩幾個。
]

#See[{nameof(IInstDict.CopyTo)}]
""")]
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex);

	[Doc($"""
#Sum[按出現口徑的鍵序逐項取值的迭代器。]

#Descr[
例：`foreach(var Kv in Dict)` 依次拿到 `Id`、`Name`、`Age` 三項；
每次迭代都現讀物件的值。
]

#See[{nameof(IInstDict.GetEnumerator)}]
""")]
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator();

	// 顯式接口實現（IEnumerable.GetEnumerator）不能標 partial，故不在聲明側列出；
	// 它只是對上面公開 GetEnumerator 的一行轉發，見 InstDict.Impl.cs。

	[Doc($"""
#Sum[用一個實例與它的型別元資料建視圖。]

#Params([[視圖背後的物件，不允許 null], [該物件的型別元資料]])

#Descr[
{nameof(Target)} 不允許 null（視圖必須能讀寫實例）。

例：`new {nameof(InstDict)}(User, Info)` 之後
`Dict["Age"] = 31` 就是寫回 `User.Age`，視圖不持有值副本。
]
""")]
	public partial InstDict(obj Target, ITypeInfo TypeInfo);
}