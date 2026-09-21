namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[Json 側的成員配接器：把官方 {nameof(JsonPropertyInfo)} 接上 {nameof(IMemberInfo)}。]

#Descr[
實測：`typeof(PoUser)` 的成員表裡每一項包的都是官方 {nameof(JsonPropertyInfo)}（首項 `Id`、末項 `Note`）。
]
""")]
public partial class JsonMemberInfo:IMemberInfo{
	[Doc($"""
#Sum[官方成員物件本體（進階用途）。]

#Descr[實測：`Age` 的這個屬性是官方 {nameof(JsonPropertyInfo)}。]
""")]
	private readonly JsonPropertyInfo _raw;

	[Doc($"""
#Sum[用官方成員物件建配接器。]

#Params([[Json, 官方成員物件；不允許 null]])
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Json);

	public partial str Name{get;}
	public partial Type PropertyType{get;}
	public partial Type DeclaringType{get;}
	public partial bool CanRead{get;}
	public partial bool CanWrite{get;}
	public partial System.Reflection.ICustomAttributeProvider? AttributeProvider{get;}
	public partial bool TryGet(obj? O, out obj? V);
	public partial bool TrySet(obj? O, obj? V);

	public partial obj Raw{get;}
}








