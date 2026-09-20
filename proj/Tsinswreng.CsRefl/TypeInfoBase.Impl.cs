namespace Tsinswreng.CsRefl;

using System.Reflection;
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
#Sum[把派生類交出的型別事實落地，成員表在此規整成契約序。]

#Descr[
實測：派生類傳進來的成員表無論是「屬性段在前、字段段在後」還是官方既有序，
都在這裡被規整成同一份契約序；以 `PoUser` 為例，出去都是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`。
]

#See[{nameof(TypeInfoBase)}]
""")]
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<obj?> Members,
		Type? ElementType,
		Type? KeyType
	){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Members);
		_type = Type;
		_kind = Kind;
		// step 1: 規整（排序 + 去重 + 只讀）：兩套來源都可能給出同名成員，
		// 詳見 TypeInfoSorter.SortEtDedup。
		_members = TypeInfoSorter.SortEtDedup(Type, Members);
		_elementType = ElementType;
		_keyType = KeyType;
	}

	[Doc($"""
#Sum[惰性建立按名索引。]

#Descr[
成員表在建構後不可變，故緩存安全。
索引雙檢：{nameof(_byName)} 是 volatile，兩個線程同時建也只會多建一份等價字典。

實測：第一次按名查時才建這份字典，故「只枚舉成員、從不按名查」的用法不付這份內存代價；
建好之後每次按名查是 O(1)。

鍵比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)} 而非默認比較：
成員名是程式碼識別符，Ordinal 才是正確語義，也不受當前文化影響。
]
""")]
	internal partial void EnsureByName(){
		if(_byName is not null){
			return;
		}
		var Dict = new Dictionary<str, obj?>(Members.Count, StringComparer.Ordinal);
		foreach(var M in Members){
			Dict[MemberExtn.Name(M)] = M;
		}
		_byName = Dict;
	}

	[Doc($"""
#Sum[按名查成員；未知返回 false。]

#Descr[
實測（`PoUser`）：`"Age"` 命中且 `{nameof(MemberExtn.Name)}` 是 "Age"；
`"NoSuch"` 返回 false、`M` 為 null 且不拋。
]

#See[{nameof(ITypeInfoExtn.TryGetMember)}]
""")]
	internal partial bool TryGetByName(str Name, out obj? M){
		EnsureByName();
		M = null;
		return _byName!.TryGetValue(Name, out M);
	}

	[Doc($"""
#Sum[按名的可用成員名清單，供未命中時的錯誤訊息用。]

#Descr[實測（`PoUser`）：11 個名，與成員表同序。]
""")]
	internal partial IEnumerable<str> AllNames(){
		return Members.Select(M => MemberExtn.Name(M));
	}
}