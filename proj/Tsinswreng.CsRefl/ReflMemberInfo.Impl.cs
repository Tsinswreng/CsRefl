namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`ReflMemberInfo` 的函數實現。]

#Descr[
只放函數實現：成員事實字段與訪問器在 `ReflMemberInfo.cs`。
]
""")]
public partial class ReflMemberInfo{
	[Doc("""
#Sum[按屬性可讀性建讀值委託。]

#Params([[要包的屬性]])

#Rtn[讀值委託；不可讀返回 null]

#Descr[
判據是「有沒有公開 get」（`GetGetMethod()` 預設只認公開訪問器），
不是 `PropertyInfo.CanRead`：
屬性帶私有 get 時 `CanRead` 仍為 true，
用它會把只寫屬性誤判成可讀
（門面承諾的是公開成員的讀寫能力）。
]
""")]
	private static Func<obj, obj?>? BuildGet(PropertyInfo Prop){
		if(Prop.GetGetMethod() is null){
			return null;
		}
		return O => Prop.GetValue(O);
	}

	[Doc("""
#Sum[按屬性可寫性建寫值委託。]

#Params([[要包的屬性]])

#Rtn[寫值委託；不可寫返回 null]

#Descr[
判據同 `BuildGet` 的說明。
]
""")]
	private static Action<obj, obj?>? BuildSet(PropertyInfo Prop){
		if(Prop.GetSetMethod() is null){
			return null;
		}
		return (O, V) => Prop.SetValue(O, V);
	}

	[Doc("""
#Sum[建字段讀值委託。]

#Params([[要包的字段]])

#Rtn[讀值委託；常量（無從取值）返回 null]
""")]
	private static Func<obj, obj?>? BuildFieldGet(FieldInfo Fld){
		if(Fld.IsLiteral){
			return null;
		}
		return O => Fld.GetValue(O);
	}

	[Doc("""
#Sum[建字段寫值委託。]

#Params([[要包的字段]])

#Rtn[寫值委託]
""")]
	private static Action<obj, obj?>? BuildFieldSet(FieldInfo Fld){
		return (O, V) => Fld.SetValue(O, V);
	}

	[Doc("""
#Sum[包一個公開實例屬性。]

#See[{nameof(ReflMemberInfo)}]
""")]
	internal partial ReflMemberInfo(PropertyInfo Prop)
		: base(
			Member: Prop,
			Json: null,
			Name: Prop.Name,
			PropertyType: Prop.PropertyType,
			DeclaringType: Prop.DeclaringType,
			// 特性提供者與官方成員對象都由 PropertyInfo 本身承擔
			// （官方 MemberInfo 就是 ICustomAttributeProvider）；
			// 成員種類轉發官方 MemberTypes.Property。
			MemberType: MemberTypes.Property,
			// 可讀/可寫與委託同一判據（委託非 null 即可讀/可寫），
			// 兩套來源統一如此，免得 CanRead 與 Get 兩處口徑打架。
			CanRead: BuildGet(Prop) is not null,
			CanWrite: BuildSet(Prop) is not null,
			Get: BuildGet(Prop),
			Set: BuildSet(Prop),
			// 特性提供者直接用 PropertyInfo 本身，調用方拿官方 GetCustomAttribute<T>() 即可。
			AttributeProvider: Prop
		)
	{
	}

	[Doc("""
#Sum[包一個公開實例字段。]

#See[{nameof(ReflMemberInfo)}]
""")]
	internal partial ReflMemberInfo(FieldInfo Fld)
		: base(
			Member: Fld,
			Json: null,
			Name: Fld.Name,
			PropertyType: Fld.FieldType,
			DeclaringType: Fld.DeclaringType,
			MemberType: MemberTypes.Field,
			// 字段的讀寫能力：常量不可讀（無從取值）、initonly/常量不可寫。
			CanRead: BuildFieldGet(Fld) is not null,
			CanWrite: !Fld.IsLiteral && !Fld.IsInitOnly,
			Get: BuildFieldGet(Fld),
			Set: BuildFieldSet(Fld),
			AttributeProvider: Fld
		)
	{
	}
}