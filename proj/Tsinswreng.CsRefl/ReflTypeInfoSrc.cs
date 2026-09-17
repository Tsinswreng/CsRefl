namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;

/// 反射來源：對任意型別用反射建立 ITypeInfo，並按型別緩存。
/// AOT 下可用——前提是查詢目標的成員元數據已被保留（見 ReflMemberInfo 的說明）。
/// RegisteredTypes 返回 null：反射來源能查任意型別，無法也無需列舉。
public partial class ReflTypeInfoSrc:ITypeInfoSrc{
	/// 型別 → 元資料緩存；同一型別只建一次（元資料構建有反射代價）。
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	/// 默認單例：不經 DI 也能直接使用的反射來源。
	public static ReflTypeInfoSrc Inst{get;} = new();

	/// 不支持列舉。
	public IReadOnlyCollection<Type>? RegisteredTypes => null;

	// TryGetInfo 實現見 ReflTypeInfoSrc.Impl.cs。
}