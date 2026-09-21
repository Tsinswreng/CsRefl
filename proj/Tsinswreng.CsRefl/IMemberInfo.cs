namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

//TswgNote 註釋寫法不合格
[Doc($$"""
#Sum[成員契約：成員名、成員型別、宿主型別、能力、讀寫——門面的成員層型別。]
#Descr[
官方兩側沒有共同的成員型別（{{nameof(JsonPropertyInfo)}} 不繼承 {{nameof(MemberInfo)}}），
故本包給成員一個契約；兩套來源各出一個配接器實現它：
{{nameof(ReflMemberInfo)}} 持官方 {{nameof(MemberInfo)}}（{{nameof(PropertyInfo)}}／{{nameof(FieldInfo)}}）、
{{nameof(JsonMemberInfo)}} 持官方 {{nameof(JsonPropertyInfo)}}。

成員從哪來：{{nameof(ITypeInfo.GetMember)}}／{{nameof(ITypeInfo.TryGetMember)}}／{{nameof(ITypeInfo.Members)}}。
按名讀寫：{{nameof(ITypeInfoExtn.TryGet)}}／{{nameof(ITypeInfoExtn.TrySet)}}。

具體取值的示例在測試域（那裏有具體模型，示例裏的型別與成員名都能 f12 跳轉）：
`proj/Tsinswreng.CsRefl.Test/Domains/Synthesis/TestMember.cs`。
]
""")]
public partial interface IMemberInfo{
	[Doc($$"""
#Sum[成員名；就是按名讀寫用的那個鍵。]
#Descr[
{{nameof(ITypeInfo.GetMember)}}／{{nameof(ITypeInfo.TryGetMember)}} 的入參就是它；
{{nameof(ITypeInfo.ReadableNames)}}／{{nameof(ITypeInfo.WritableNames)}} 列出的也是它。
]
""")]
	str Name{get;}

	[Doc($$"""
#Sum[成員自身的型別：成員聲明時寫的那個型別，與運行期裝的值無關。]
#Descr[
聲明為 {{nameof(System.Collections.Generic.List<str>)}}<str> 的成員，本屬性就是它；
此刻裝進去的值不影響本屬性。

按名查同一個事實見 {{nameof(ITypeInfo.TryGetMemberType)}}。
]
""")]//TswgNote
	Type PropertyType{get;}

	[Doc($$"""
#Sum[成員的宿主型別：成員聲明在哪個型別裏。]
#Descr[
繼承來的成員，本屬性是聲明它的那個基類，不是取到它的那個型別。

取成員時用的型別見 {{nameof(ITypeInfo.Type)}}。
]
""")]//TswgNote
	Type DeclaringType{get;}

	[Doc($$"""
#Sum[本成員可否讀取。]
#Descr[
判據是成員自身有沒有公開的讀取器，不是「在不在某張鍵表裏」。
只寫成員為 false，讀它要經 {{nameof(IMemberInfo.TryGet)}}，那裏同樣返回 false。

整份名單見 {{nameof(ITypeInfo.ReadableNames)}}。
]
""")]
	bool CanRead{get;}

	[Doc($$"""
#Sum[本成員可否寫入。]
#Descr[
判據是成員自身有沒有公開的寫入器，不是「在不在某張鍵表裏」。
只讀成員為 false，寫它要經 {{nameof(IMemberInfo.TrySet)}}，那裏同樣返回 false。

整份名單見 {{nameof(ITypeInfo.WritableNames)}}。
]
""")]
	bool CanWrite{get;}

	[Doc($$"""
#Sum[官方特性提供者（兩側共有的官方接口 {{nameof(ICustomAttributeProvider)}}）。]
#Descr[
反射側的成員物件本身就是它；Json 側取官方
{{nameof(JsonPropertyInfo)}}.{{nameof(JsonPropertyInfo.AttributeProvider)}}。

取特性走 {{nameof(AttrProvider.GetCustomAttribute)}}。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;}

	[Doc($$"""
#Sum[讀取實例上的本成員；讀不到返回 false（不拋）。]
#Params([[O, 實例；null 或與宿主型別不符時返回 false], [V, 讀出的值；失敗時為 null]])
#Descr[
按名一步讀值的捷徑是 {{nameof(ITypeInfoExtn.TryGet)}}。

返回 false 的場合：實例為 null、實例與 {{nameof(IMemberInfo.DeclaringType)}} 不符、成員只寫、
名不在成員表上。
]
""")]
	bool TryGet(obj? O, out obj? V);

	[Doc($$"""
#Sum[寫入實例上的本成員；寫不進返回 false（不拋）。]
#Params([[O, 實例；null 或與宿主型別不符時返回 false], [V, 要寫入的值]])
#Descr[
按名一步寫值的捷徑是 {{nameof(ITypeInfoExtn.TrySet)}}。

返回 false 的場合：實例為 null、實例與 {{nameof(IMemberInfo.DeclaringType)}} 不符、成員只讀、
名不在成員表上；值型別不符照常拋（那是調用方的 bug）。
]
""")]
	bool TrySet(obj? O, obj? V);
}




