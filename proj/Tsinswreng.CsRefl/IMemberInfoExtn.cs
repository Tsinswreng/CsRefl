namespace Tsinswreng.CsRefl;

using System.Reflection;

/// IMemberInfo 的擴展：特性查詢。
///
/// 為甚麼需要這一層：官方把 GetCustomAttribute&lt;T&gt;() 只掛在具體型別上
/// （MemberInfo／Assembly／Module／ParameterInfo／PropertyInfo），
/// 而官方在 Json 與反射兩側**共有**的接口 ICustomAttributeProvider 上並沒有它。
/// 門面的 AttributeProvider 正是 ICustomAttributeProvider，故補一個與官方同形的
/// 便利方法（名字、返回 null 的約定都照官方 CustomAttributeExtensions），
/// 讓調用方不必自己寫 GetCustomAttributes(typeof(T), false) 的樣板。
/// 實現見 IMemberInfoExtn.Impl.cs。
public static partial class IMemberInfoExtn{
	/// 取本成員上第一個 TAttr 型別的特性；沒有返回 null（與官方
	/// CustomAttributeExtensions.GetCustomAttribute&lt;T&gt; 同語義：不繼承、不拋）。
	public static partial TAttr? GetCustomAttribute<TAttr>(this IMemberInfo z) where TAttr:Attribute;
}