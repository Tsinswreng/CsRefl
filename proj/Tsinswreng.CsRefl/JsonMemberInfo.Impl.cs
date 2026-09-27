namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonMemberInfo)} 的函數實現。]
""")]
public partial class JsonMemberInfo{
	[Doc($"""
#Sum[用官方成員物件建配接器：構造期把成員事實取一次落在自動屬性上。]
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Json){
		ArgumentNullException.ThrowIfNull(Json);
		// step 1: 官方成員物件本體留在 _Raw，供進階用途直接使官方源生成 API。
		_Raw = Json;
		// step 2: 成員事實構造期一次算好，直接落在自動屬性上（自動屬性可再賦值，賦值＝換掉那件事實）。
		Name = Json.Name;
		PropertyType = Json.PropertyType;
		OwnerType = Json.DeclaringType;
		// step 3: 能力判據與官方讀寫委託同一條：那兩條委託在不在。
		CanRead = Json.Get is not null;
		CanWrite = Json.Set is not null;
		// step 4: 官方成員物件自帶的特性提供者。
		AttributeProvider = Json.AttributeProvider;
	}

	// 肏你媽臭屄這東西是給你寫到impl裏的嗎?
	// 已按此改：訪問器本體（Name／PropertyType／OwnerType／CanRead／CanWrite／AttributeProvider）
	// 全部搬回 JsonMemberInfo.cs（Decl），Impl 只留函數。

	[Doc($"""
#Sum[現讀官方 {nameof(JsonPropertyInfo)} 的讀取委託取實例上的值。]

#See[{nameof(IMemberInfo.TryGet)}]
""")]
	public partial bool TryGet(obj? O, out obj? V){
		V = null;
		// step 1: 讀值的三道前置：實例非 null、成員可讀、實例與宿主型別相符。
		if(O is null || !CanRead || !OwnerType.IsInstanceOfType(O)){
			return false;
		}
		// step 2: 取官方那條委託；CanRead 已保證非 null，這裡只是收窄可空性。
		var Get = _Raw.Get;
		if(Get is null){
			return false;
		}
		// step 3: 真正取值；值型別不符由官方委託拋（InvalidCastException），不吞成 false。
		V = Get(O);
		return true;
	}

	[Doc($"""
#Sum[現讀官方 {nameof(JsonPropertyInfo)} 的寫入委託寫入實例上的值。]

#See[{nameof(IMemberInfo.TrySet)}]
""")]
	public partial bool TrySet(obj? O, obj? V){
		// step 1: 寫值的三道前置：實例非 null、成員可寫、實例與宿主型別相符。
		if(O is null || !CanWrite || !OwnerType.IsInstanceOfType(O)){
			return false;
		}
		// step 2: 取官方那條委託；CanWrite 已保證非 null，這裡只是收窄可空性。
		var Set = _Raw.Set;
		if(Set is null){
			return false;
		}
		// step 3: 真正寫入；值型別不符由官方委託拋，不吞成 false——那是調用方的 bug。
		Set(O, V);
		return true;
	}

	//TswgNote 肏你媽臭屄又脫褲子放屁了是吧? 而且這東西是給你寫到impl裏的嗎?
	// 已按此改：欄位改 public readonly JsonPropertyInfo _Raw，唯讀轉發屬性 Raw 已刪。
}




