namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonMemberInfo)} 的函數實現（先佔位）。]
""")]
public partial class JsonMemberInfo{
	public partial JsonMemberInfo(JsonPropertyInfo Json){
		_Raw = Json;
		throw new NotImplementedException();
	}

	// 肏你媽臭屄這東西是給你寫到impl裏的嗎?
	// 已按此改：訪問器本體（Name／PropertyType／DeclaringType／CanRead／CanWrite／AttributeProvider）
	// 全部搬回 JsonMemberInfo.cs（Decl），Impl 只留函數。

	public partial bool TryGet(obj? O, out obj? V){
		throw new NotImplementedException();
	}

	public partial bool TrySet(obj? O, obj? V){
		throw new NotImplementedException();
	}

	//TswgNote 肏你媽臭屄又脫褲子放屁了是吧? 而且這東西是給你寫到impl裏的嗎?
	// 已按此改：欄位改 public readonly JsonPropertyInfo _Raw，唯讀轉發屬性 Raw 已刪。
}




