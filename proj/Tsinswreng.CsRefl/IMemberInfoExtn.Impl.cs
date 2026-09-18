namespace Tsinswreng.CsRefl;

/// IMemberInfoExtn 的函數實現。
/// 只放函數實現：簽名在 IMemberInfoExtn.cs。
public static partial class IMemberInfoExtn{
	/// 取第一個匹配的特性；提供者缺失或沒有匹配都返回 null。
	/// 直接轉官方 ICustomAttributeProvider.GetCustomAttributes，
	/// 第二個實參 false 與官方 MemberInfo.GetCustomAttribute&lt;T&gt;() 一致（不繼承）。
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute{
		if(z is null){
			return null;
		}
		var Provider = z.AttributeProvider;
		if(Provider is null){
			return null;
		}
		foreach(var A in Provider.GetCustomAttributes(typeof(TAttr), false)){
			if(A is TAttr T){
				return T;
			}
		}
		return null;
	}
}