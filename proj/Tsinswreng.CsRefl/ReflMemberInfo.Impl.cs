namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ReflMemberInfo)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `ReflMemberInfo.cs`。
]
""")]
public partial class ReflMemberInfo{
	[Doc($"""
#Sum[用官方成員物件建配接器：構造期把成員事實取一次落在自動屬性上。]

#Descr[
只認 {nameof(PropertyInfo)} 與 {nameof(FieldInfo)}：認不得的官方成員種類沒有「成員型別、可讀可寫」可言，
故當場拋而不是留一個半殘的配接器（本包自己只會用這兩種造配接器，見 {nameof(ReflTypeInfo)} 收集成員的那一步）。
]
""")]
	public partial ReflMemberInfo(MemberInfo Member){
		ArgumentNullException.ThrowIfNull(Member);
		// step 1: 官方成員物件本體留在 _Raw，供進階用途直接使官方反射 API。
		_Raw = Member;
		// step 2: 成員事實構造期一次算好，直接落在自動屬性上（自動屬性可再賦值，賦值＝換掉那件事實）。
		Name = Member.Name;
		OwnerType = Member.DeclaringType!;
		// step 3: 屬性與字段是兩種官方成員物件，各自取自己的型別與可讀可寫判據。
		if(Member is PropertyInfo Prop){
			PropertyType = Prop.PropertyType;
			// 判據與官方讀寫委託同一條：公開 getter／setter 方法在不在。
			CanRead = Prop.GetGetMethod() is not null;
			CanWrite = Prop.GetSetMethod() is not null;
		}else if(Member is FieldInfo Fld){
			PropertyType = Fld.FieldType;
			// 公開字段恆可讀可寫。
			CanRead = true;
			CanWrite = true;
		}else{
			throw new NotSupportedException(
				$"本配接器只接 {nameof(PropertyInfo)} 與 {nameof(FieldInfo)}，收到 {Member.GetType().Name}。"
			);
		}
		// step 4: 官方成員物件本身就是官方特性提供者。
		AttributeProvider = Member;
	}

	[Doc($"""
#Sum[現讀官方 {nameof(PropertyInfo)}／{nameof(FieldInfo)} 取實例上的值。]

#See[{nameof(IMemberInfo.TryGet)}]
""")]
	public partial bool TryGet(obj? O, out obj? V){
		V = null;
		// step 1: 讀值的三道前置：實例非 null、成員可讀、實例與宿主型別相符。
		if(O is null || !CanRead || !OwnerType.IsInstanceOfType(O)){
			return false;
		}
		// step 2: 屬性與字段各走官方自己的取值入口。
		if(_Raw is PropertyInfo Prop){
			V = Prop.GetValue(O);
			return true;
		}
		if(_Raw is FieldInfo Fld){
			V = Fld.GetValue(O);
			return true;
		}
		// step 3: 既非屬性也非字段（構造子已擋掉，此分支不可達）：按「讀不到」處理，不拋。
		return false;
	}

	[Doc($"""
#Sum[現讀官方 {nameof(PropertyInfo)}／{nameof(FieldInfo)} 寫入實例上的值。]

#See[{nameof(IMemberInfo.TrySet)}]
""")]
	public partial bool TrySet(obj? O, obj? V){
		// step 1: 寫值的三道前置：實例非 null、成員可寫、實例與宿主型別相符。
		if(O is null || !CanWrite || !OwnerType.IsInstanceOfType(O)){
			return false;
		}
		// step 2: 屬性與字段各走官方自己的寫值入口；
		//         值型別不符由官方反射拋（ArgumentException），不吞成 false——那是調用方的 bug。
		if(_Raw is PropertyInfo Prop){
			Prop.SetValue(O, V);
			return true;
		}
		if(_Raw is FieldInfo Fld){
			Fld.SetValue(O, V);
			return true;
		}
		// step 3: 既非屬性也非字段（構造子已擋掉，此分支不可達）：按「寫不進」處理，不拋。
		return false;
	}
}
