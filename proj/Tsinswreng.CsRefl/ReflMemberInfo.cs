namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射側的成員配接器：把官方 {nameof(MemberInfo)}（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}）接上 {nameof(IMemberInfo)}。]

#Descr[
屬性走官方 {nameof(PropertyInfo)} 的成員事實、字段走官方 {nameof(FieldInfo)} 的成員事實，
兩者都經本類統一成門面的 {nameof(IMemberInfo)}。

成員事實（名、型別、宿主、可讀可寫、特性提供者）在構造期向官方成員物件取一次，直接落在自動屬性上；
之後官方物件再變也不回頭看——要現讀官方那一份，就用 {nameof(_Raw)}。
]
""")]
public partial class ReflMemberInfo:IMemberInfo{
	[Doc($"""
#Sum[官方成員物件本體（進階用途）。]

#Descr[
構造期賦值，之後不再改；要直接使官方反射 API，用它。
]
""")]
	public readonly MemberInfo _Raw;

	[Doc($"""
#Sum[用官方成員物件建配接器。]

#Params([[Member, 官方成員物件；不允許 null，且只接 {nameof(PropertyInfo)} 與 {nameof(FieldInfo)}]])
""")]
	public partial ReflMemberInfo(MemberInfo Member);

	[Doc($"""
#Sum[成員名，構造期取自官方成員物件。]

#See[{nameof(IMemberInfo.Name)}]
""")]
	public str Name{get;set;}

	[Doc($"""
#Sum[成員型別，構造期取官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.PropertyType)} 或 {nameof(FieldInfo)}.{nameof(FieldInfo.FieldType)}。]

#See[{nameof(IMemberInfo.PropertyType)}]
""")]
	public Type PropertyType{get;set;}

	[Doc($"""
#Sum[成員宣告所在的型別，構造期取自官方成員物件。]

#See[{nameof(IMemberInfo.OwnerType)}]
""")]
	public Type OwnerType{get;set;}

	[Doc($"""
#Sum[構造期看官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.GetGetMethod)} 是否非 null；字段恆為 true。]

#See[{nameof(IMemberInfo.CanRead)}]
""")]
	public bool CanRead{get;set;}

	[Doc($"""
#Sum[構造期看官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.GetSetMethod)} 是否非 null；字段恆為 true。]

#See[{nameof(IMemberInfo.CanWrite)}]
""")]
	public bool CanWrite{get;set;}

	[Doc($"""
#Sum[官方成員物件本身就是官方特性提供者。]

#See[{nameof(IMemberInfo.AttributeProvider)}]
""")]
	public ICustomAttributeProvider? AttributeProvider{get;set;}

	public partial bool TryGet(obj? O, out obj? V);
	public partial bool TrySet(obj? O, obj? V);
}
