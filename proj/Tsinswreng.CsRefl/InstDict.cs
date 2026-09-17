namespace Tsinswreng.CsRefl;

/// 一個物件的「淺字典視圖」。鍵 = 可讀且可寫的成員（按成員序），
/// 讀 = 從物件讀，寫 = 寫回物件；Add/Remove/Clear 拋 NotSupportedException
/// （形狀由型別固定），只有改既有鍵的值合法。
/// 建構子見 InstDict.Impl.cs。
public partial class InstDict:IInstDict{
	/// 視圖背後的物件。
	private readonly obj _target;
	/// 視圖所用到的型別元資料。
	private readonly ITypeInfo _typeInfo;
	/// 鍵清單：可讀且可寫的成員名，按成員序（與 SQL 列序、前端欄位序保持一致；
	/// 修掉舊 PropDict 用排序集合把順序改成字母序的問題）。
	private readonly List<str> _keys;

	public obj? Target => _target;
	public ITypeInfo TypeInfo => _typeInfo;

	public bool IsReadOnly => false;
	public int Count => _keys.Count;
	public ICollection<str> Keys => _keys;
	/// 每次訪問現算（按鍵序取一遍），不做排序（修掉舊 PropDict 對異質值排序會拋的問題）。
	public ICollection<obj?> Values => BuildValues();

	/// 讀寫器。讀：鍵不存在或不可讀 → KeyNotFoundException；
	/// 寫：鍵不存在 → KeyNotFoundException，不可寫 → InvalidOperationException，
	/// 值型別不符 → InvalidOperationException。
	public obj? this[str Key]{
		get{
			return ReadCell(Key);
		}
		set{
			WriteCell(Key, value);
		}
	}

	// 其餘 IDictionary 成員（ContainsKey/TryGetValue/Add/Remove/Clear/CopyTo/GetEnumerator）見 InstDict.Impl.cs。
}