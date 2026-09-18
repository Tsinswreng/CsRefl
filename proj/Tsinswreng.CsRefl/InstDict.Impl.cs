namespace Tsinswreng.CsRefl;

using System.Collections.ObjectModel;

/// InstDict 的函數實現。
/// 只放函數實現：字段與訪問器在 InstDict.cs。
/// 注意：顯式接口實現（IEnumerable.GetEnumerator）不能標 partial，
/// 故留在本文件；它只是對公開 GetEnumerator 的一行轉發。
public partial class InstDict{
	/// 用一個實例與它的型別元資料建視圖。
	/// 鍵只收「可讀且可寫」的成員：只讀成員寫不進去、只寫成員讀不出來，
	/// 兩者放進字典視圖都會讓 IDictionary 的讀寫契約自相矛盾。
	public partial InstDict(obj Target, ITypeInfo TypeInfo){
		ArgumentNullException.ThrowIfNull(Target);
		ArgumentNullException.ThrowIfNull(TypeInfo);
		_target = Target;
		_typeInfo = TypeInfo;
		var Keys = new List<str>();
		foreach(var M in TypeInfo.Members){
			if(M.CanRead && M.CanWrite){
				Keys.Add(M.Name);
			}
		}
		_keys = Keys;
		// 活視圖包裝（不複製）：外部拿不到 List 的 Add/Remove，形狀改不動。
		_keysView = new ReadOnlyCollection<str>(Keys);
	}

	/// 讀寫各按成員能力放行；實現見 ReadCell / WriteCell。
	public partial obj? this[str Key]{
		get{
			return ReadCell(Key);
		}
		set{
			WriteCell(Key, value);
		}
	}

	/// 按鍵讀；成員不存在或不可讀拋 KeyNotFoundException（訊息含出現口徑的鍵）。
	/// 判據是成員表而非 _keys：只讀成員也讀得到（見 InstDict 的口徑說明）。
	private obj? ReadCell(str Key){
		if(!_typeInfo.TryGetMember(Key, out var M) || !M.CanRead){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏（不存在或不可讀）。可用鍵：{string.Join(", ", _keys)}");
		}
		if(!M.TryGet(_target, out var R)){
			throw new KeyNotFoundException($"讀取成員 {Key} 失敗（實例型別不符）。");
		}
		return R;
	}

	/// 按鍵寫；成員不存在拋 KeyNotFoundException、不可寫或值型別不符拋 InvalidOperationException。
	/// 判據同樣是成員表：只寫成員寫得進（它讀不出來，故不在 _keys 裏）。
	private void WriteCell(str Key, obj? Value){
		if(!_typeInfo.TryGetMember(Key, out var M)){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏（不存在）。可用鍵：{string.Join(", ", _keys)}");
		}
		if(!M.CanWrite){
			throw new InvalidOperationException($"成員 {Key} 不可寫。");
		}
		if(!M.TrySet(_target, Value)){
			throw new InvalidOperationException($"寫入成員 {Key} 失敗（實例型別或值型別不符）。");
		}
	}

	/// 逐鍵求值，不做排序。
	private ICollection<obj?> BuildValues(){
		var R = new List<obj?>(_keys.Count);
		foreach(var K in _keys){
			R.Add(ReadCell(K));
		}
		return R;
	}

	/// 鍵是否存在於本視圖。
	public partial bool ContainsKey(str Key){
		return _keys.Contains(Key);
	}

	/// 取值；按成員表判定，未知成員返回 false（只讀成員也取得到）。
	public partial bool TryGetValue(str Key, out obj? Value){
		if(_typeInfo.TryGetMember(Key, out var M)
			&& M.CanRead
			&& M.TryGet(_target, out var R))
		{
			Value = R;
			return true;
		}
		Value = default;
		return false;
	}

	/// 形狀由型別成員固定，不支持新增鍵。
	public partial void Add(str Key, obj? Value){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	/// 形狀由型別成員固定，不支持新增鍵。
	public partial void Add(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	/// 形狀由型別成員固定，不支持刪鍵。
	public partial bool Remove(str Key){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	/// 形狀由型別成員固定，不支持刪鍵。
	public partial bool Remove(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	/// 形狀由型別成員固定，不支持清空。
	public partial void Clear(){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持清空。");
	}

	/// 鍵值對是否都在視圖內且相等。
	public partial bool Contains(KeyValuePair<str, obj?> Item){
		return TryGetValue(Item.Key, out var V) && EqualityComparer<obj?>.Default.Equals(V, Item.Value);
	}

	/// 按鍵序拷貝鍵值對到數組。
	/// 先驗容量與下標（ICollection 的既有約定），否則失敗時只會裸拋 IndexOutOfRange，
	/// 對調用方毫無線索。
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex){
		ArgumentNullException.ThrowIfNull(Array);
		ArgumentOutOfRangeException.ThrowIfNegative(ArrayIndex);
		if(Array.Length - ArrayIndex < _keys.Count){
			throw new ArgumentException(
				$"目標數組容量不足：從下標 {ArrayIndex} 起需要 {_keys.Count} 個位置，實際只剩 {Array.Length - ArrayIndex} 個。",
				nameof(Array)
			);
		}
		foreach(var K in _keys){
			Array[ArrayIndex] = new KeyValuePair<str, obj?>(K, ReadCell(K));
			ArrayIndex++;
		}
	}

	/// 按鍵序逐項取值的迭代器。
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator(){
		foreach(var K in _keys){
			yield return new KeyValuePair<str, obj?>(K, ReadCell(K));
		}
	}

	/// 顯式接口實現（不能標 partial）：轉發到公開 GetEnumerator。
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return GetEnumerator();
	}
}