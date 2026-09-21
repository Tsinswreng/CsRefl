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
#Sum[把派生類交出的型別事實落地，成員表在此規整成契約序。]

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
		// step 1: 構造期算出來的事實直接落在屬性上（自動屬性），不另存欄位由屬性轉發。
		// 參數名與屬性同名，故這裡必須用 this. 指定賦值目標。
		this.Type = Type;
		this.Kind = Kind;
		// step 2: 規整（排序 + 去重 + 只讀）：兩套來源都可能給出同名成員，
		// 詳見 TypeInfoSorter.SortEtDedup。
		this.Members = TypeInfoSorter.SortEtDedup(Type, Members);
		this.ElementType = ElementType;
		this.KeyType = KeyType;
	}

	[Doc($"""
#Sum[按名查成員；走惰性索引，O(1)。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, out IMemberInfo? M){
		M = null;
		// step 1: 名字為 null 時直接返回 false（成員名不可能是 null，故這不是「查不到」而是「沒法查」）。
		if(Name is null){
			return false;
		}
		// step 2: 走索引（第一次調用時才建，見 EnsureByName）。
		EnsureByName();
		return _byName!.TryGetValue(Name, out M);
	}

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name){
		ArgumentNullException.ThrowIfNull(Name);
		// step 1: 命中就返回；未命中才付「列可用名」的代價（錯誤路徑）。
		if(TryGetMember(Name, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {Type.FullName} 沒有成員 {Name}。可用成員：{string.Join(", ", AllNames())}"
		);
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
	private partial void EnsureByName(){
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
#Sum[按成員序列出全部成員名，供未命中時的錯誤訊息用。]

#Descr[
實測（`PoUser`）：11 個名，與成員表同序。
]
""")]
	private partial IEnumerable<str> AllNames(){
		return Members.Select(M => M.Name);
	}

	public partial bool TryGetMemberType(str Name, out Type? T){
		throw new NotImplementedException();
	}

	public partial bool CanRead(str Name){
		throw new NotImplementedException();
	}

	public partial bool CanWrite(str Name){
		throw new NotImplementedException();
	}
}









