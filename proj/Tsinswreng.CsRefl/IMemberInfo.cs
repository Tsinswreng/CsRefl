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

#Descr[
事實成員皆可賦值：賦值＝換掉那件事實（不合併、不拷貝、不驗證），留給調用方自行取用。
兩個配接器（{{nameof(ReflMemberInfo)}}／{{nameof(JsonMemberInfo)}}）在構造期向官方成員物件取一次事實、
直接落在自動屬性上，故賦值＝換掉那一次取到的值；官方物件之後再變不會回頭看（要現讀就用它們的 `_Raw`）。
]
""")]
public partial interface IMemberInfo{
	[Doc($$"""
#Sum[成員名；就是按名讀寫用的那個鍵。]
#Descr[
{{nameof(ITypeInfo.GetMember)}}／{{nameof(ITypeInfo.TryGetMember)}} 的入參就是它；
{{nameof(ITypeInfo.ReadableMembers)}}／{{nameof(ITypeInfo.WritableMembers)}}／{{nameof(ITypeInfo.ReadWriteMembers)}} 的鍵也是它。
]
""")]
	str Name{get;set;}

	[Doc($$"""
#Sum[這個屬性或字段是甚麼型別。]
#Descr[
一個屬性或字段在型別定義裏寫的是甚麼型別，{{nameof(IMemberInfo.PropertyType)}} 就是甚麼型別，
與成員裏當下存着甚麼值無關。

本例取 Name 這個屬性，Name 定義成 str，故取到的型別是 str，與 Name 此刻裝着的值無關。

下面例子用到：{{nameof(ITypeInfoSrcExtn.GetInfo)}}、{{nameof(IMemberInfo)}}、{{nameof(ITypeInfo.GetMember)}}、{{nameof(IMemberInfo.Name)}}、{{nameof(IMemberInfo.PropertyType)}}。

src 是一個來源。

```cs
var info = src.GetInfo(typeof(IMemberInfo));
var m = info.GetMember(nameof(IMemberInfo.Name));

var t = m.PropertyType;   // str
```
]
""")]//TswgNote
	Type PropertyType{get;set;}

	[Doc($$"""
#Sum[這個屬性或字段屬於哪個型別。]
#Descr[
一個屬性或字段的定義寫在哪個型別裏，{{nameof(IMemberInfo.OwnerType)}} 就是那個型別。
繼承來的成員，定義寫在基類，故 {{nameof(IMemberInfo.OwnerType)}} 是基類，不是取到該成員的子類。

本例取 JsonTypeInfoInfo 的 Members，這個屬性宣告在 JsonTypeInfoInfo 自己身上，
故取到的是 JsonTypeInfoInfo。

下面例子用到：{{nameof(ITypeInfoSrcExtn.GetInfo)}}、{{nameof(JsonTypeInfoInfo)}}、{{nameof(ITypeInfo.GetMember)}}、{{nameof(JsonTypeInfoInfo.Members)}}、{{nameof(IMemberInfo.OwnerType)}}。

src 是一個來源。

```cs
var info = src.GetInfo(typeof(JsonTypeInfoInfo));
var m = info.GetMember(nameof(JsonTypeInfoInfo.Members));

var d = m.OwnerType;   // JsonTypeInfoInfo
```
]
""")]//TswgNote
	Type OwnerType{get;set;}

	[Doc($$"""
#Sum[本成員可否讀取。]
#Descr[
判據是成員自身有沒有公開的讀取器，不是「在不在某張鍵表裏」。
只寫成員為 false，讀它要經 {{nameof(TryGet)}}，那裏同樣返回 false。

整份名單見 {{nameof(ITypeInfo.ReadableMembers)}}。
]
""")]
	bool CanRead{get;set;}

	[Doc($$"""
#Sum[本成員可否寫入。]
#Descr[
判據是成員自身有沒有公開的寫入器，不是「在不在某張鍵表裏」。
只讀成員為 false，寫它要經 {{nameof(TrySet)}}，那裏同樣返回 false。

整份名單見 {{nameof(ITypeInfo.WritableMembers)}}。
]
""")]
	bool CanWrite{get;set;}

	[Doc($$"""
#Sum[官方特性提供者（兩側共有的官方接口 {{nameof(ICustomAttributeProvider)}}）。]
#Descr[
反射側的成員物件本身就是它；Json 側取官方
{{nameof(JsonPropertyInfo)}}.{{nameof(JsonPropertyInfo.AttributeProvider)}}。

取特性走 {{nameof(AttrProvider.GetCustomAttribute)}}。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;set;}

	[Doc($$"""
#Sum[讀取實例上的本成員；讀不到返回 false（不拋）。]
#Params([[O, 實例；null 或與宿主型別不符時返回 false], [V, 讀出的值；失敗時為 null]])
#Descr[
按名一步讀值的捷徑是 {{nameof(ITypeInfoExtn.TryGet)}}。

返回 false 的場合：實例為 null、實例與 {{nameof(OwnerType)}} 不符、成員只寫、
名不在成員表上。
]
""")]
	bool TryGet(obj? O, out obj? V);

	[Doc($$"""
#Sum[寫入實例上的本成員；寫不進返回 false（不拋）。]
#Params([[O, 實例；null 或與宿主型別不符時返回 false], [V, 要寫入的值]])
#Descr[
按名一步寫值的捷徑是 {{nameof(ITypeInfoExtn.TrySet)}}。

返回 false 的場合：實例為 null、實例與 {{nameof(OwnerType)}} 不符、成員只讀、
名不在成員表上；值型別不符照常拋（那是調用方的 bug）。
]
""")]
	bool TrySet(obj? O, obj? V);
}




