namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`TypeInfoBase` 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `TypeInfoBase.cs`。
]
""")]
public abstract partial class TypeInfoBase{
	[Doc("""
#Sum[把派生類交出的型別事實落地。]

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

	[Doc("""
#Sum[惰性建立按名索引。]

#Descr[
`Members` 在建構後不可變，故緩存安全。
索引雙檢：`_byName` 是 volatile，兩個線程同時建也只會多建一份等價字典。
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

	[Doc("""
#Sum[按名查成員；未知返回 false。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, out IMemberInfo? M){
		EnsureByName();
		M = null;
		return _byName!.TryGetValue(Name, out M);
	}

	[Doc("""
#Sum[按名取成員；未知拋 `KeyNotFoundException`，訊息含可用名清單。]

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