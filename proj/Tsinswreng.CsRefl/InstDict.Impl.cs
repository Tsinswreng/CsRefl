namespace Tsinswreng.CsRefl;

using System.Collections.ObjectModel;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(InstDict)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `InstDict.cs`。

注意：顯式接口實現（{nameof(System.Collections.IEnumerable)}.{nameof(System.Collections.IEnumerable.GetEnumerator)}）
不能標 `partial`，故留在本文件；
它只是對公開 {nameof(GetEnumerator)} 的一行轉發。
]
""")]
public partial class InstDict{
	[Doc($"""
#Sum[用一個實例與它的型別元資料建視圖。]

#See[{nameof(InstDict)}]
""")]
	public partial InstDict(obj Target, ITypeInfo TypeInfo){
		ArgumentNullException.ThrowIfNull(Target);
		ArgumentNullException.ThrowIfNull(TypeInfo);
		// step 1: 記住視圖背後的物件與型別元資料，後續讀寫都落在這兩者上。
		// 兩者直接落在屬性上（自動屬性），不另存欄位由屬性轉發；參數同名，故用 this.。
		this.Target = Target;
		this.TypeInfo = TypeInfo;
		// step 2: 收集出現口徑的鍵。
		// 鍵只收「可讀且可寫」的成員：只讀成員寫不進去、只寫成員讀不出來，
		// 兩者放進字典視圖都會讓 IDictionary 的讀寫契約自相矛盾。
		var KeyList = new List<str>();
		var KeySet = new HashSet<str>(StringComparer.Ordinal);
		foreach(var M in TypeInfo.Members){
			if(Member.CanRead(M) && Member.CanWrite(M)){
				var N = Member.Name(M);
				KeyList.Add(N);
				KeySet.Add(N);
			}
		}
		_keys = KeyList;
		_keySet = KeySet;
		// step 3: 對外只讀包裝（不複製）：外部拿不到 List 的 Add/Remove，形狀改不動。
		Keys = new ReadOnlyCollection<str>(KeyList);
	}

	[Doc($"""
#Sum[讀寫各按成員能力放行。]

#Descr[
實測（`PoUser`，`Age = 26`）：`Dict["Age"]` 讀到 boxed 的 `i32` 26；
`Dict["Level"] = 8` 之後 `User.Level` 是 8；
`Dict["Secret"]` 讀到 "s"（只讀成員讀得到）、`Dict["Token"] = "t1"` 寫得進，
而 `Secret` 與 `Token` 都不在 {nameof(Keys)} 裏。
]

#See[{nameof(InstDict)}]
""")]
	public partial obj? this[str Key]{
		get{
			return ReadCell(Key);
		}
		set{
			WriteCell(Key, value);
		}
	}

	[Doc($"""
#Sum[按鍵讀。]

#Params([[Key, 要讀的鍵]])

#Rtn[讀出的值]

#Descr[
成員不存在或不可讀拋 {nameof(KeyNotFoundException)}（訊息含出現口徑的鍵）。
判據是成員表而非 {nameof(_keys)}：只讀成員也讀得到（見 {nameof(InstDict)} 的口徑說明）。

實測（`PoUser`，`Secret = "s"`）：讀 `"Secret"` 返回 "s"；
讀 `"Token"`（只寫）拋 {nameof(KeyNotFoundException)}，
讀 `"NoSuch"` 也拋 {nameof(KeyNotFoundException)}；
訊息裏列出的是可讀可寫那批鍵（實測含 "Level" 可被斷言），
故「為甚麼這個成員讀不到」要回去看成員表的 {nameof(Member.CanRead)}，不是看鍵表。
]
""")]
	private partial obj? ReadCell(str Key){
		// step 1: 成員必須存在且可讀（判據是成員表，不看鍵表）。
		if(!TypeInfo.TryGetMember(Key, out var M) || !Member.CanRead(M)){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏（不存在或不可讀）。可用鍵：{string.Join(", ", _keys)}");
		}
		// step 2: 真正取值；實例型別不符時 TryGet 返回 false。
		if(!Member.TryGet(M, Target, out var R)){
			throw new KeyNotFoundException($"讀取成員 {Key} 失敗（實例型別不符）。");
		}
		return R;
	}

	[Doc($"""
#Sum[按鍵寫。]

#Params([[Key, 要寫的鍵], [Value, 要寫入的值]])

#Rtn[無返回值；失敗時拋異常]

#Descr[
成員不存在拋 {nameof(KeyNotFoundException)}，
不可寫或值型別不符拋 {nameof(InvalidOperationException)}。

判據同樣是成員表：只寫成員寫得進（它讀不出來，故不在 {nameof(_keys)} 裏）。

實測（`PoUser`）：寫 `"Level"` 為 8 成功且寫回物件；
寫 `"Secret"` 拋 {nameof(InvalidOperationException)}，訊息指名 "Secret"（成員在、但不可寫）；
寫 `"NoSuch"` 拋 {nameof(KeyNotFoundException)}，訊息含可用鍵，兩種錯分得開。
]
""")]
	private partial void WriteCell(str Key, obj? Value){
		// step 1: 成員必須存在。
		if(!TypeInfo.TryGetMember(Key, out var M)){
			throw new KeyNotFoundException($"鍵 {Key} 不在字典視圖裏（不存在）。可用鍵：{string.Join(", ", _keys)}");
		}
		// step 2: 成員必須可寫（只讀成員到這裡就被擋住，不會撞到底層異常）。
		if(!Member.CanWrite(M)){
			throw new InvalidOperationException($"成員 {Key} 不可寫。");
		}
		// step 3: 真正寫入；實例型別或值型別不符時 TrySet 返回 false。
		if(!Member.TrySet(M, Target, Value)){
			throw new InvalidOperationException($"寫入成員 {Key} 失敗（實例型別或值型別不符）。");
		}
	}

	[Doc($"""
#Sum[按出現口徑的鍵序逐鍵取值。]

#Rtn[值集合，順序同 {nameof(_keys)}]

#Descr[
逐鍵求值，不做排序。

實測（`PoUser`）：得到的值是 9 個，依次為 boxed 的 `i64` 1、`str` "小明"、boxed 的 `i32` 26、
null、false、空列表、空字典、boxed 的 `i32` 0、`str` "n"；
`i64`、`str`、集合混在一起照樣平鋪返回，
因為這裡只取值不比較，故不會像舊 `PropDict` 那樣因異質值不可互比而拋。
]
""")]
	private partial ICollection<obj?> BuildValues(){
		var R = new List<obj?>(_keys.Count);
		foreach(var K in _keys){
			R.Add(ReadCell(K));
		}
		return R;
	}

	[Doc($"""
#Sum[鍵是否存在於本視圖。]

#See[{nameof(InstDict.ContainsKey)}]
""")]
	public partial bool ContainsKey(str Key){
		// 走 _keySet（O(1)），不掃 _keys（那是 O(n)，鍵一多就變貴）。
		return _keySet.Contains(Key);
	}

	[Doc($"""
#Sum[取值；按成員表判定，未知成員返回 false（只讀成員也取得到）。]

#See[{nameof(InstDict.TryGetValue)}]
""")]
	public partial bool TryGetValue(str Key, out obj? Value){
		if(TypeInfo.TryGetMember(Key, out var M)
			&& Member.CanRead(M)
			&& Member.TryGet(M, Target, out var R))
		{
			Value = R;
			return true;
		}
		Value = default;
		return false;
	}

	[Doc($"""
#Sum[形狀由型別成員固定，不支持新增鍵。]

#See[{nameof(InstDict.Add)}]
""")]
	public partial void Add(str Key, obj? Value){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	[Doc($"""
#Sum[形狀由型別成員固定，不支持新增鍵。]

#See[{nameof(InstDict.Add)}]
""")]
	public partial void Add(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持新增鍵；請在型別上加可寫成員。");
	}

	[Doc($"""
#Sum[形狀由型別成員固定，不支持刪鍵。]

#See[{nameof(InstDict.Remove)}]
""")]
	public partial bool Remove(str Key){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	[Doc($"""
#Sum[形狀由型別成員固定，不支持刪鍵。]

#See[{nameof(InstDict.Remove)}]
""")]
	public partial bool Remove(KeyValuePair<str, obj?> Item){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持刪鍵。");
	}

	[Doc($"""
#Sum[形狀由型別成員固定，不支持清空。]

#Descr[
實測：`Dict.{nameof(Clear)}()` 恆拋 {nameof(NotSupportedException)}；
視圖的鍵就是型別的成員，清不掉（真要「全部歸零」得逐個成員
{nameof(Member)}.{nameof(Member.TrySet)} 成默認值）。
]

#See[{nameof(InstDict.Clear)}]
""")]
	public partial void Clear(){
		throw new NotSupportedException("字典視圖的形狀由型別成員固定，不支持清空。");
	}

	[Doc($"""
#Sum[鍵值對是否都在視圖內且相等。]

#See[{nameof(InstDict.Contains)}]
""")]
	public partial bool Contains(KeyValuePair<str, obj?> Item){
		return TryGetValue(Item.Key, out var V) && EqualityComparer<obj?>.Default.Equals(V, Item.Value);
	}

	[Doc($"""
#Sum[按鍵序拷貝鍵值對到數組。]

#See[{nameof(InstDict.CopyTo)}]
""")]
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex){
		// 先驗容量與下標（ICollection 的既有約定），否則失敗時只會裸拋 IndexOutOfRange，
		// 對調用方毫無線索。
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

	[Doc($"""
#Sum[按鍵序逐項取值的迭代器。]

#Descr[
實測：`foreach(var Kv in Dict)` 依次拿到 9 對，
第一對是 `Id` 配 boxed 的 `i64` 1、第七對的鍵是 `Extra`、最後一對的鍵是 `Note`；
每次迭代都現讀物件的值，故迭代期間物件被改動時看到的是改後的值。
]

#See[{nameof(InstDict.GetEnumerator)}]
""")]
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator(){
		foreach(var K in _keys){
			yield return new KeyValuePair<str, obj?>(K, ReadCell(K));
		}
	}

	// 顯式接口實現（不能標 partial）：轉發到公開 GetEnumerator。
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return GetEnumerator();
	}
}

