namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonMemberInfo)} 的函數實現（先佔位）。]
""")]
public partial class JsonMemberInfo{
	public partial JsonMemberInfo(JsonPropertyInfo Json){
		_raw = Json;
		throw new NotImplementedException();
	}

	// 肏你媽臭屄這東西是給你寫到impl裏的嗎?
	public partial str Name{ get{ throw new NotImplementedException(); } }
	public partial Type PropertyType{ get{ throw new NotImplementedException(); } }
	public partial Type DeclaringType{ get{ throw new NotImplementedException(); } }
	public partial bool CanRead{ get{ throw new NotImplementedException(); } }
	public partial bool CanWrite{ get{ throw new NotImplementedException(); } }
	public partial System.Reflection.ICustomAttributeProvider? AttributeProvider{ get{ throw new NotImplementedException(); } }

	public partial bool TryGet(obj? O, out obj? V){
		throw new NotImplementedException();
	}

	public partial bool TrySet(obj? O, obj? V){
		throw new NotImplementedException();
	}

	//TswgNote 肏你媽臭屄又脫褲子放屁了是吧? 而且這東西是給你寫到impl裏的嗎?
	public partial obj Raw{ get{ return _raw; } }
}





