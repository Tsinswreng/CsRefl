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
判據與 {nameof(CanMkInst)} 同一條：本配接器的 {nameof(CreateObject)} 是否為 null
（沒賦值過時它就是官方那條委託）。
]
""")]
	public partial obj? MkInst(){
		// 取本配接器的那個屬性（可能被賦值覆蓋過），而不是直接讀官方本體。
		var F = CreateObject;
		if(F is null){
			throw new NotSupportedException(
				$"型別 {Type.FullName} 沒有可用的 {nameof(CreateObject)}（官方元資料沒給，或已被賦值成 null），無法建立實例。"
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

	[Doc($"""
#Sum[按名取成員的宣告型別。]

#See[{nameof(ITypeInfo.TryGetMemberType)}]
""")]
	public partial bool TryGetMemberType(str Name, out Type? T){
		T = null;
		// step 1: 按名查成員表（本身就是保序字典，O(1)）；名字為 null 時直接 false。
		if(Name is null || !Members.TryGetValue(Name, out var M)){
			return false;
		}
		// step 2: 型別由成員本體給（兩套來源的成員契約同一條口徑）。
		T = M.PropertyType;
		return true;
	}

	[Doc($"""
#Sum[按名問成員能不能讀。]

#See[{nameof(ITypeInfo.CanRead)}]
""")]
	public partial bool CanRead(str Name){
		// 成員不存在（含名字為 null）就是「不能讀」；能力判據在成員本體上。
		return Name is not null && Members.TryGetValue(Name, out var M) && M.CanRead;
	}

	[Doc($"""
#Sum[按名問成員能不能寫。]

#See[{nameof(ITypeInfo.CanWrite)}]
""")]
	public partial bool CanWrite(str Name){
		// 成員不存在（含名字為 null）就是「不能寫」；能力判據在成員本體上。
		return Name is not null && Members.TryGetValue(Name, out var M) && M.CanWrite;
	}
}
