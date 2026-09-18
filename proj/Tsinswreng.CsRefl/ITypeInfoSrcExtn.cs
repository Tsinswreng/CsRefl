namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// 操作層：在 ITypeInfoSrc 之上的按名讀寫與物件↔淺字典互轉。
/// 這裡是調用方（CsSql、Ngan.Dict 業務）真正接觸的面：來源只負責「查型別」，
/// 本類負責把查詢結果拼成好用的一次調用。
///
/// 參數名的選擇：成員名參數一律叫 Name，與官方 MemberInfo.Name /
/// JsonPropertyInfo.Name 同名（門面的「鍵」就是官方那個名字，沒有另造概念）。
/// 實現見 ITypeInfoSrcExtn.Impl.cs（傳統擴展方法語法支持 partial 拆分）。
public static partial class ITypeInfoSrcExtn{
	/// 運行期型別的 DAM 擔保：動態 GetType() 在分析器眼裏不攜帶 DAM 信息，但本包
	/// 在此路徑上對 T 的用法只有 IsInstanceOfType / 反射建元資料，缺元數據時會在
	/// 反射建元資料處自然拋錯，不會悄悄剪錯，故此處顯式擔保成員元數據需求。
	[UnconditionalSuppressMessage("Trimming", "IL2068",
		Justification = "運行期型別（O.GetType()）本質無法靜態攜帶 DAM 信息；本包對它的用法只有 IsInstanceOfType 與反射建元資料，缺元數據時在反射建元資料處自然拋錯，不會悄悄剪錯。")]
	[return: DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)]
	private static partial Type RuntimeType(Type T);

	/// 取成員；型別未註冊或成員不存在都拋 KeyNotFoundException（訊息含線索）。
	/// Type 的 DAM 註解：後端會把型別交給來源查元資料（反射來源需要成員元數據）。
	public static partial IMemberInfo GetMember(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name
	);
	/// Try 版：型別未註冊 / 成員不存在都返回 false。
	public static partial bool TryGetMember(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		[NotNullWhen(true)] out IMemberInfo? M
	);
	/// 按名讀值；型別/成員/實例/可讀性任一不滿足返回 false。
	public static partial bool TryGet(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		obj? O,
		out obj? R
	);
	/// 按名寫值；型別/成員/實例/可寫性任一不滿足返回 false。
	public static partial bool TrySet(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		obj? O,
		obj? V
	);
	/// 建淺字典視圖，型別取 O.GetType()（執行期型別，修掉舊 Srefl 用 typeof(T)
	/// 查不到介面/基類成員的隱患）。O 為 null 拋 ArgumentNullException。
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O);
	/// 建淺字典視圖，型別由調用方顯式給（Type 為 null 時退到 O.GetType()）。
	public static partial IInstDict ToInstDict(
		this ITypeInfoSrc z,
		obj? O,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type? Type
	);
	/// 把字典寫回物件（型別取 O.GetType()）：按名逐鍵 TrySet，只讀成員跳過；
	/// 鍵不在型別上 → KeyNotFoundException，值型別不符 → InvalidOperationException。
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict);
	/// 把字典寫回物件，型別由調用方顯式給。
	public static partial void AssignFromDict(
		this ITypeInfoSrc z,
		obj? O,
		IReadOnlyDictionary<str, obj?> Dict,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type? Type
	);
}