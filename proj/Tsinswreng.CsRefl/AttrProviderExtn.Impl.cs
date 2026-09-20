namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(AttrProviderExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `AttrProviderExtn.Decl.cs`。
]
""")]
public static partial class AttrProviderExtn{
	public static partial TAttr? GetCustomAttribute<TAttr>(this ICustomAttributeProvider? z) where TAttr:Attribute{
		// step 1: 提供者缺失（成員沒有元資料或沒給提供者）→ null，不拋。
		if(z is null){
			return null;
		}
		// step 2: 直接轉官方 ICustomAttributeProvider.GetCustomAttributes；
		// 第二個實參 false 與官方 MemberInfo.GetCustomAttribute<T>() 一致（不繼承）。
		foreach(var A in z.GetCustomAttributes(typeof(TAttr), false)){
			if(A is TAttr T){
				return T;
			}
		}
		return null;
	}
}