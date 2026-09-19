namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(TypeInfoBase)} 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `TypeInfoBase.cs`。
]
""")]
public abstract partial class TypeInfoBase{
	[Doc($"""
#Sum[把派生類交出的型別事實落地。]

#Descr[
例：派生類傳進來的成員表無論是「屬性段在前、字段段在後」還是官方既有序，
都在這裡被規整成同一份契約序，
故之後 {nameof(Members)} 的順序與來源無關。
]

#See[{nameof(TypeInfoBase)}]
""")]
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<IMemberInfo> Members,
		Type? ElementType,
		Type? KeyType
	){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Members);
		_type = Type;
		_kind = Kind;
		// 規整（排序 + 去重 + 只讀）：兩套來源都可能給出同名成員，
		// 詳見 TypeInfoSorter.SortEtDedup。
		_members = TypeInfoSorter.SortEtDedup(Type, Members);
		_elementType = ElementType;
		_keyType = KeyType;
	}

	[Doc($"""
#Sum[惰性建立按名索引。]

#Descr[
{nameof(Members)} 在建構後不可變，故緩存安全。
索引雙檢：{nameof(_byName)} 是 volatile，兩個線程同時建也只會多建一份等價字典。

例：第一次 {nameof(TryGetMember)} 時才建這份字典，
故「只枚舉 {nameof(Members)}、從不按名查」的用法不付這份內存代價；
建好之後每次按名查是 O(1)。

用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)} 而非默認比較：
成員名是程式碼識別符，Ordinal 才是正確語義，也不受當前文化影響。
]
""")]
	private void EnsureByName(){
		if(_byName is not null){
			return;
		}
		var Dict = new Dictionary<str, IMemberInfo>(Members.Count, StringComparer.Ordinal);
		foreach(var M in Members){
			Dict[M.Name] = M;
		}
		_byName = Dict;
	}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#Descr[
例：`Info.{nameof(TryGetMember)}("Age", out var M)` 命中；
`Info.{nameof(TryGetMember)}("NoSuch", out _)` 返回 false 且不拋。
]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, out IMemberInfo? M){
		EnsureByName();
		M = null;
		return _byName!.TryGetValue(Name, out M);
	}

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}，訊息含可用名清單。]

#Descr[
例：`Info.{nameof(GetMember)}("NoSuch")` 拋出的訊息形如
「型別 Xxx 沒有成員 NoSuch。可用成員：Id, Name, Age」，
直接把可用名擺出來，不必另去查 {nameof(Members)} 排查拼寫。

訊息裏現算 {nameof(Members)} 的名字清單，故只在失敗路徑付這個代價。
]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name){
		EnsureByName();
		if(_byName!.TryGetValue(Name, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {Type.FullName} 沒有成員 {Name}。可用成員：{string.Join(", ", Members.Select(X => X.Name))}"
		);
	}
}