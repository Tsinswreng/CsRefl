namespace Tsinswreng.CsRefl;

/// 一個物件的「淺字典視圖」。
///
/// 口徑（兩條，職責不同，不可混為一談）：
/// - 出現口徑（Keys / Count / Values / 枚舉）：可讀且可寫的成員——這些是視圖
///   「表現為字典」時呈現的鍵，順序 = 成員序；
/// - 訪問口徑（索引器 / TryGetValue / ContainsKey）：讀寫各按成員自身能力放行，
///   Secret 這類只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。
///   兩者的差別是故意的：字典視圖要能當普通字典改值，又不該因為某成員只讀
///   就把它的值藏起來（見 TestInstDict 的「鍵表外的可讀成員允許讀」用例）。
///
/// 都不在成員表上的名字才叫「不存在」：讀拋 KeyNotFoundException、
/// ContainsKey 返回 false。形狀由型別成員固定，因而 Add / Remove / Clear 拋
/// NotSupportedException，只有改既有成員的值是合法的。
///
/// 用法即 Srefl 時代的 PropDict，但修掉了兩個已知缺陷：
/// - 鍵序 = 成員序（舊實現用排序集合，把順序改成字母序）；
/// - Values 不排序（舊實現把異質值塞進排序集合，值型別不可互比時會拋）。
/// 建構子與 IDictionary 成員的實現見 InstDict.Impl.cs。
public partial class InstDict:IInstDict{
	/// 視圖背後的物件。
	private readonly obj _target;
	/// 視圖所用到的型別元資料。
	private readonly ITypeInfo _typeInfo;
	/// 出現口徑的鍵清單：可讀且可寫的成員名，按成員序（與 SQL 列序、前端欄位序
	/// 保持一致；修掉舊 PropDict 用排序集合把順序改成字母序的問題）。
	private readonly List<str> _keys;
	/// Keys 對外只讀包裝：活視圖（與 _keys 同一份數據，不複製），但不許外部改形狀。
	private readonly ICollection<str> _keysView;

	/// 讀寫各按成員能力放行的實現見 InstDict.Impl.cs：
	/// 讀要求可讀、寫要求可寫，兩者的判據都是成員表而非 _keys。
	public partial obj? this[str Key]{
		get;
		set;
	}

	/// 視圖背後的物件。
	public obj? Target{
		get{
			return _target;
		}
	}
	/// 視圖所用到的型別元資料。
	public ITypeInfo TypeInfo{
		get{
			return _typeInfo;
		}
	}

	/// 視圖可被寫入既有成員，故不是只讀字典（IsReadOnly 的語義見 IDictionary）。
	public bool IsReadOnly{
		get{
			return false;
		}
	}
	/// 出現口徑的鍵數 = 可讀可寫成員數。
	public int Count{
		get{
			return _keys.Count;
		}
	}
	/// 出現口徑的鍵集合；活視圖，與視圖同一份鍵集合，但不提供改形狀的入口。
	public ICollection<str> Keys{
		get{
			return _keysView;
		}
	}
	/// 每次訪問現算（按出現口徑的鍵序取一遍），不做排序（修掉舊 PropDict 對異質值排序會拋的問題）。
	public ICollection<obj?> Values{
		get{
			return BuildValues();
		}
	}

	/// 鍵是否存在於出現口徑（= 可讀且可寫的成員）。
	public partial bool ContainsKey(str Key);
	/// 按成員表取值：成員存在且可讀即成功；未知成員返回 false。
	public partial bool TryGetValue(str Key, out obj? Value);
	/// 形狀由型別成員固定，本操作恆拋 NotSupportedException。
	public partial void Add(str Key, obj? Value);
	/// 形狀由型別成員固定，本操作恆拋 NotSupportedException。
	public partial void Add(KeyValuePair<str, obj?> Item);
	/// 形狀由型別成員固定，本操作恆拋 NotSupportedException。
	public partial bool Remove(str Key);
	/// 形狀由型別成員固定，本操作恆拋 NotSupportedException。
	public partial bool Remove(KeyValuePair<str, obj?> Item);
	/// 形狀由型別成員固定，本操作恆拋 NotSupportedException。
	public partial void Clear();
	/// 鍵值對是否都在視圖內且相等。
	public partial bool Contains(KeyValuePair<str, obj?> Item);
	/// 按出現口徑的鍵序拷貝鍵值對到數組；下標為負或容量不足時拋 ArgumentException 系列。
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex);
	/// 按出現口徑的鍵序逐項取值的迭代器。
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator();
	// 顯式接口實現（IEnumerable.GetEnumerator）不能標 partial，故不在聲明側列出；
	// 它只是對上面公開 GetEnumerator 的一行轉發，見 InstDict.Impl.cs。

	/// 用一個實例與它的型別元資料建視圖。Target 不允許 null（視圖必須能讀寫實例）。
	public partial InstDict(obj Target, ITypeInfo TypeInfo);
}