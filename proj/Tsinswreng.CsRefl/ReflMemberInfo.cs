namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射側的成員配接器：把官方 {nameof(MemberInfo)}（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}）接上 {nameof(IMemberInfo)}。]

#Descr[
實測：`typeof(PoUser)` 的成員表裡，`Id` 包的是官方 {nameof(PropertyInfo)}、`Note` 包的是官方 {nameof(FieldInfo)}。
]
""")]
public partial class ReflMemberInfo:IMemberInfo{
	[Doc($"""
#Sum[官方成員物件本體（進階用途）。]

#Descr[
實測：`Age` 的這個屬性是官方 {nameof(PropertyInfo)}；`Note` 的是官方 {nameof(FieldInfo)}。
]
""")]
	private readonly MemberInfo _raw;

	[Doc($"""
#Sum[用官方成員物件建配接器。]

#Params([[Member, 官方成員物件；不允許 null]])
""")]
	public partial ReflMemberInfo(MemberInfo Member);

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








