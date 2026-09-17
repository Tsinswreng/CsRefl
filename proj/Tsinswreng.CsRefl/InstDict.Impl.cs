namespace Tsinswreng.CsRefl;

/// InstDict 的函數實現。
public partial class InstDict{
	/// 用一個實例與它的型別元資料建視圖。Target 不允許 null（視圖必須能讀寫實例）。
	public InstDict(obj Target, ITypeInfo TypeInfo){
		ArgumentNullException.ThrowIfNull(Target);
		ArgumentNullException.ThrowIfNull(TypeInfo);
		_target = Target;
		_typeInfo = TypeInfo;
		var Keys = new List<str>();
		foreach(var M in TypeInfo.Members){
			if(M.CanRead && M.CanWrite){
				Keys.Add(M.CodeName);
			}
		}
		_keys = Keys;
	}

	/// 按鍵讀；鍵不存在或不可讀拋 KeyNotFoundException（訊息含可用鍵）。
	private obj? ReadCell(str Key){
		if(!_typeInfo.TryGetMember(Key, out var M) || !M.CanRead){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏（不存在或不可讀）。可用鍵：{string.Join(", ", _keys)}");
		}
		if(!M.TryGet(_target, out var R)){
			throw new KeyNotFoundException($"讀取成員 {Key} 失敗（實例型別不符）。");
		}
		return R;
	}

	/// 按鍵寫；鍵不存在拋 KeyNotFoundException、不可寫/值型別不符拋 InvalidOperationException。
	private void WriteCell(str Key, obj? Value){
		if(!_typeInfo.TryGetMember(Key, out var M)){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏。可用鍵：{string.Join(", ", _keys)}");
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

	public bool ContainsKey(str Key){
		return _keys.Contains(Key);
	}

	public bool TryGetValue(str Key, out obj? Value){
		if(_typeInfo.TryGetMember(Key, out var M) && M.CanRead && M.TryGet(_target, out var R)){
			Value = R;
			return true;
		}
		Value = default;
		return false;
	}

	public void Add(str Key, obj? Value){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	public void Add(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	public bool Remove(str Key){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	public bool Remove(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	public void Clear(){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持清空。");
	}

	public bool Contains(KeyValuePair<str, obj?> Item){
		return TryGetValue(Item.Key, out var V) && EqualityComparer<obj?>.Default.Equals(V, Item.Value);
	}

	public void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex){
		ArgumentNullException.ThrowIfNull(Array);
		foreach(var K in _keys){
			Array[ArrayIndex] = new KeyValuePair<str, obj?>(K, ReadCell(K));
			ArrayIndex++;
		}
	}

	public IEnumerator<KeyValuePair<str, obj?>> GetEnumerator(){
		foreach(var K in _keys){
			yield return new KeyValuePair<str, obj?>(K, ReadCell(K));
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return GetEnumerator();
	}
}