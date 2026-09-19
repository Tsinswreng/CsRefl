namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(IMemberInfoExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `IMemberInfoExtn.cs`。
]
""")]
public static partial class IMemberInfoExtn{
	[Doc($"""
#Sum[取第一個匹配的特性；提供者缺失或沒有匹配都返回 null。]

#Descr[
例：成員上標了自訂特性時返回那個實例；
沒標返回 null；`z` 為 null 也返回 null（擴展方法在 null 上可調，不拋）。
]

#See[{nameof(IMemberInfoExtn.GetCustomAttribute)}]
""")]
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute{
		// step 1: 提供者缺失：沒有成員元資料或沒給提供者，直接返回 null，不拋。
		if(z is null){
			return null;
		}
		var Provider = z.AttributeProvider;
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