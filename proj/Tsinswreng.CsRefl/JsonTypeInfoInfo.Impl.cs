namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonTypeInfoInfo)} 的函數實現。]

#Descr[
只放函數實現：屬性與字段在 `JsonTypeInfoInfo.cs`。
]
""")]
public partial class JsonTypeInfoInfo{
	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
基礎事實全部取自官方那一個實例，成員表取官方 {nameof(JsonTypeInfo.Properties)}，
每一項包成 {nameof(JsonMemberInfo)}；之後排序去重成契約序。
]
""")]
	public partial JsonTypeInfoInfo(JsonTypeInfo Json){
		ArgumentNullException.ThrowIfNull(Json);
		// step 1: 基礎事實全部取自官方那一個實例。
		this.Type = Json.Type;
		this.Kind = Json.Kind;
		this.ElementType = Json.ElementType;
		this.KeyType = Json.KeyType;
		// step 2: 官方 Properties 是 IList<JsonPropertyInfo>，逐項包成 IMemberInfo，再規整成契約序。
		this.Members = TypeInfoSorter.SortEtDedup(
			Json.Type,
			Json.Properties.Select(P => (IMemberInfo)new JsonMemberInfo(P)).ToList()
		);
		// step 3: 官方本體直接落在屬性上（不再另存欄位轉發）；參數同名，故用 this.。
		this.Json = Json;
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
實測：`typeof(PoUser)` 建得出實例；沒有官方工廠的型別拋 {nameof(NotSupportedException)}，訊息含型別全名。
]
""")]
	public partial obj? MkInst(){
		// 錯誤訊息與 CanMkInst 用同一個判據：官方 CreateObject 是否為 null。
		var J = Json!;
		var F = J.CreateObject;
		if(F is null){
			throw new NotSupportedException(
				$"型別 {J.Type.FullName} 的官方元資料沒有 CreateObject，無法建立實例。"
			);
		}
		return F();
	}

	[Doc($"""
#Sum[惰性建立按名索引。]

#Descr[
成員表在建構後不可變，故緩存安全。
索引雙檢：{nameof(_ByName)} 是 volatile，兩個線程同時建也只會多建一份等價字典。

實測：第一次按名查時才建這份字典，故「只枚舉成員、從不按名查」的用法不付這份內存代價；
建好之後每次按名查是 O(1)。

鍵比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)} 而非默認比較：
成員名是程式碼識別符，Ordinal 才是正確語義，也不受當前文化影響。
]
""")]
	private partial void EnsureByName(){
		if(_ByName is not null){
			return;
		}
		var Dict = new Dictionary<str, IMemberInfo>(Members.Count, StringComparer.Ordinal);
		foreach(var M in Members){
			Dict[M.Name] = M;
		}
		_ByName = Dict;
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
		return _ByName!.TryGetValue(Name, out M);
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
