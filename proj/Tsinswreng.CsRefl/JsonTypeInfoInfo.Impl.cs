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
		// step 2: 官方 Properties 是 IList<JsonPropertyInfo>，逐項包成 IMemberInfo，規整成契約序，
		//         再依序收進保序字典：插入序即契約序，故成員表的枚舉順序就是契約序。
		var Sorted = TypeInfoSorter.SortEtDedup(
			Json.Type,
			Json.Properties.Select(P => (IMemberInfo)new JsonMemberInfo(P)).ToList()
		);
		var MemberDict = new OrderedDictionary<str, IMemberInfo>(Sorted.Count);
		foreach(var M in Sorted){
			MemberDict[M.Name] = M;
		}
		this.Members = MemberDict;
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
#Sum[由成員表過濾出一份子集快照。]

#See[{nameof(MkSubset)}]
""")]
	private partial IDictionary<str, IMemberInfo> MkSubset(Func<IMemberInfo, bool> Pick){
		var R = new OrderedDictionary<str, IMemberInfo>();
		foreach(var (Name, M) in Members){
			if(Pick(M)){
				R[Name] = M;
			}
		}
		return R;
	}

	[Doc($"""
#Sum[按名查成員；就是成員表的一次字典查，O(1)。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, out IMemberInfo? M){
		M = null;
		// step 1: 名字為 null 時直接返回 false（成員名不可能是 null，故這不是「查不到」而是「沒法查」）。
		if(Name is null){
			return false;
		}
		// step 2: 成員表本身是保序字典（見 Members），按名查就是它的一次字典查。
		return Members.TryGetValue(Name, out M);
	}

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name){
		ArgumentNullException.ThrowIfNull(Name);
		// step 1: 命中就返回；未命中才付「列可用名」的代價（錯誤路徑）。
		if(Members.TryGetValue(Name, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {Type.FullName} 沒有成員 {Name}。可用成員：{string.Join(", ", Members.Keys)}"
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
