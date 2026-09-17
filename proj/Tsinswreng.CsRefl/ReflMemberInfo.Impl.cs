namespace Tsinswreng.CsRefl;

using System.Reflection;

/// ReflMemberInfo 的函數實現。
public partial class ReflMemberInfo{
	/// 按屬性可讀性建讀值委託；不可讀返回 null。
	private static Func<obj, obj?>? BuildGet(PropertyInfo Prop){
		if(!Prop.CanRead){
			return null;
		}
		return O => Prop.GetValue(O);
	}

	/// 按屬性可寫性建寫值委託；不可寫返回 null。
	private static Action<obj, obj?>? BuildSet(PropertyInfo Prop){
		if(!Prop.CanWrite){
			return null;
		}
		return (O, V) => Prop.SetValue(O, V);
	}

	/// 字段讀值委託；常量（無從取值）返回 null。
	private static Func<obj, obj?>? BuildFieldGet(FieldInfo Fld){
		if(Fld.IsLiteral){
			return null;
		}
		return O => Fld.GetValue(O);
	}

	/// 字段寫值委託。
	private static Action<obj, obj?>? BuildFieldSet(FieldInfo Fld){
		return (O, V) => Fld.SetValue(O, V);
	}

	/// 包一個公開實例屬性。Order 為收集序（見 ReflTypeInfo.CollectMembers）。
	internal ReflMemberInfo(PropertyInfo Prop, i32 Order)
		: base(BuildGet(Prop), BuildSet(Prop))
	{
		_codeName = Prop.Name;
		_jsonName = null;
		_kind = EMemberKind.Property;
		_declaredType = Prop.PropertyType;
		_declaringType = Prop.DeclaringType!;
		_canRead = Prop.CanRead;
		_canWrite = Prop.CanWrite;
		_order = Order;
		_attrs = Prop.GetCustomAttributes().Cast<Attribute>().ToList();
	}

	/// 包一個公開實例字段。
	internal ReflMemberInfo(FieldInfo Fld, i32 Order)
		: base(BuildFieldGet(Fld), BuildFieldSet(Fld))
	{
		_codeName = Fld.Name;
		_jsonName = null;
		_kind = EMemberKind.Field;
		_declaredType = Fld.FieldType;
		_declaringType = Fld.DeclaringType!;
		// 字段的讀寫能力：常量不可讀（無從取值）、initonly/常量不可寫。
		_canRead = !Fld.IsLiteral;
		_canWrite = !Fld.IsLiteral && !Fld.IsInitOnly;
		_order = Order;
		_attrs = Fld.GetCustomAttributes().Cast<Attribute>().ToList();
	}
}