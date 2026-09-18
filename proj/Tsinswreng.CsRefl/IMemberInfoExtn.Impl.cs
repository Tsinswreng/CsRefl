namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc("""
#Sum[`IMemberInfoExtn` 的函數實現。]

#Descr[
只放函數實現：簽名在 `IMemberInfoExtn.cs`。
]
""")]
public static partial class IMemberInfoExtn{
	[Doc("""
#Sum[取第一個匹配的特性；提供者缺失或沒有匹配都返回 null。]

#See[{nameof(IMemberInfoExtn.GetCustomAttribute)}]
""")]
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute{
		// 提供者缺失：沒有成員元資料或沒給提供者，直接返回 null，不拋。
		if(z is null){
			return null;
		}
		var Provider = z.AttributeProvider;
		if(Provider is null){
			return null;
		}
		// 直接轉官方 ICustomAttributeProvider.GetCustomAttributes；
		// 第二個實參 false 與官方 MemberInfo.GetCustomAttribute<T>() 一致（不繼承）。
		foreach(var A in Provider.GetCustomAttributes(typeof(TAttr), false)){
			if(A is TAttr T){
				return T;
			}
		}
		return null;
	}
}