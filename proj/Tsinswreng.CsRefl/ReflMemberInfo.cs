namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射側的成員配接器：把官方 {nameof(MemberInfo)}（{nameof(PropertyInfo)} 或 {nameof(FieldInfo)}）接上 {nameof(IMemberInfo)}。]

#Descr[
屬性走官方 {nameof(PropertyInfo)} 的成員事實、字段走官方 {nameof(FieldInfo)} 的成員事實，
兩者都經本類統一成門面的 {nameof(IMemberInfo)}。
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

#Params([[Member, 官方成員物件；不允許 null]])
""")]
	public partial ReflMemberInfo(MemberInfo Member);

	[Doc($"""
#Sum[成員名，取自官方成員物件。]

#See[{nameof(IMemberInfo.Name)}]
""")]
	public str Name{
		get{
			return _Raw.Name;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[屬性取官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.PropertyType)}，字段取官方 {nameof(FieldInfo)}.{nameof(FieldInfo.FieldType)}。]

#See[{nameof(IMemberInfo.PropertyType)}]
""")]
	public Type PropertyType{
		get{
			if(_Raw is PropertyInfo prop){
				return prop.PropertyType;
			}
			if(_Raw is FieldInfo fld){
				return fld.FieldType;
			}
			throw new NotSupportedException($"本配接器只接 {nameof(PropertyInfo)} 與 {nameof(FieldInfo)}。");
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[成員宣告所在的型別，取自官方成員物件。]

#See[{nameof(IMemberInfo.OwnerType)}]
""")]
	public Type OwnerType{
		get{
			return _Raw.DeclaringType!;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[屬性看官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.GetGetMethod)} 是否非 null；字段恆為 true。]

#See[{nameof(IMemberInfo.CanRead)}]
""")]
	public bool CanRead{
		get{
			if(_Raw is PropertyInfo prop){
				return prop.GetGetMethod() is not null;
			}
			return _Raw is FieldInfo;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[屬性看官方 {nameof(PropertyInfo)}.{nameof(PropertyInfo.GetSetMethod)} 是否非 null；字段恆為 true。]

#See[{nameof(IMemberInfo.CanWrite)}]
""")]
	public bool CanWrite{
		get{
			if(_Raw is PropertyInfo prop){
				return prop.GetSetMethod() is not null;
			}
			return _Raw is FieldInfo;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[官方成員物件本身就是官方特性提供者。]

#See[{nameof(IMemberInfo.AttributeProvider)}]
""")]
	public ICustomAttributeProvider? AttributeProvider{
		get{
			return _Raw;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	public partial bool TryGet(obj? O, out obj? V);
	public partial bool TrySet(obj? O, obj? V);
}




