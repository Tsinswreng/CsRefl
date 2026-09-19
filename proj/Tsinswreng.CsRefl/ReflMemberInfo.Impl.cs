namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ReflMemberInfo)} 的函數實現。]

#Descr[
只放函數實現：成員事實字段與訪問器在 `ReflMemberInfo.cs`。
]
""")]
public partial class ReflMemberInfo{
	private static partial Func<obj, obj?>? BuildGet(PropertyInfo Prop){
		// 沒有公開 get 就是不可讀（不看 CanRead，它會把私有 get 也算可讀）。
		if(Prop.GetGetMethod() is null){
			return null;
		}
		return O => Prop.GetValue(O);
	}

	private static partial Action<obj, obj?>? BuildSet(PropertyInfo Prop){
		if(Prop.GetSetMethod() is null){
			return null;
		}
		return (O, V) => Prop.SetValue(O, V);
	}

	private static partial Func<obj, obj?>? BuildFieldGet(FieldInfo Fld){
		if(Fld.IsLiteral){
			return null;
		}
		return O => Fld.GetValue(O);
	}

	private static partial Action<obj, obj?>? BuildFieldSet(FieldInfo Fld){
		return (O, V) => Fld.SetValue(O, V);
	}

	[Doc($"""
#Sum[包一個公開實例屬性。]

#Descr[
實測（`PoUser` 的 `Age`）：{nameof(ReflMemberInfo.Name)} 是 "Age"、
{nameof(ReflMemberInfo.Member)} 是那個 {nameof(PropertyInfo)}、
{nameof(ReflMemberInfo.Json)} 為 null、{nameof(ReflMemberInfo.AttributeProvider)} 也是那個 {nameof(PropertyInfo)}。
]

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

	[Doc($"""
#Sum[包一個公開實例字段。]

#Descr[
實測（`PoUser` 的 `Note`）：{nameof(ReflMemberInfo.MemberType)} 是
{nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
若那個字段是 `readonly`，則 {nameof(ReflMemberInfo.CanWrite)} 為 false、{nameof(ReflMemberInfo.CanRead)} 仍為 true。
]

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