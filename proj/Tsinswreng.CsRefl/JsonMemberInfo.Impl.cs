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

	public partial obj Raw{ get{ return _raw; } }
}





