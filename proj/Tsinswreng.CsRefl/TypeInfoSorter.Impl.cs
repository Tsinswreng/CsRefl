namespace Tsinswreng.CsRefl;

/// TypeInfoSorter 的函數實現。
internal static partial class TypeInfoSorter{
	/// 基類在前穩定排序：DepthOf（到 Root 的繼承鏈長，基類更深）降序。
	/// LINQ OrderBy 穩定：同深度保持原有先後。
	public static IReadOnlyList<IMemberInfo> Sort(Type Root, IEnumerable<IMemberInfo> Members){
		ArgumentNullException.ThrowIfNull(Root);
		ArgumentNullException.ThrowIfNull(Members);
		return Members
			.OrderByDescending(M => DepthOf(Root, M.DeclaringType))
			.ToList();
	}

	/// Declaring 相對於 Root 的繼承深度（Root 自身為 0）。
	private static int DepthOf(Type Root, Type Declaring){
		var D = 0;
		var T = Declaring;
		while(T is not null && T != Root){
			T = T.BaseType;
			D++;
		}
		return D;
	}
}