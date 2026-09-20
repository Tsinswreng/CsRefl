namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(AttrProvider)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `AttrProvider.Decl.cs`。
]
""")]
public static partial class AttrProvider{
	public static partial TAttr? GetCustomAttribute<TAttr>(ICustomAttributeProvider? Provider) where TAttr:Attribute{
		// step 1: 提供者缺失（成員沒有元資料或沒給提供者）→ null，不拋。
		if(Provider is null){
			return null;
		}
		// step 2: 直接轉官方 ICustomAttributeProvider.GetCustomAttributes；
		// 第二個實參 false 與官方 MemberInfo.GetCustomAttribute<T>() 一致（不繼承）。
		foreach(var A in Provider.GetCustomAttributes(typeof(TAttr), false)){
			if(A is TAttr T){
				return T;
			}
		}
		return null;
	}
}



