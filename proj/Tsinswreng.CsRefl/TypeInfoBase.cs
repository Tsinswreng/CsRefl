namespace Tsinswreng.CsRefl;

/// 型別元資料的公共基類：把「按名查成員」「名清單」這些只依賴 Members 的公共邏輯
/// 集中實現，兩套來源只需提供 Type/Kind/Members/實例工廠/鍵值型別。
/// 函數實現（TryGetMember/GetMember）見 TypeInfoBase.Impl.cs。
public abstract partial class TypeInfoBase:ITypeInfo{
	/// 按 CodeName 的成員索引緩存，首次查詢時建立。
	private Dictionary<str, IMemberInfo>? _byName;
	/// 可讀名清單緩存。
	private IReadOnlyCollection<str>? _readable;
	/// 可寫名清單緩存。
	private IReadOnlyCollection<str>? _writable;

	public abstract Type Type{get;}
	public abstract ETypeKind Kind{get;}
	public abstract bool CanMkInst{get;}
	public abstract obj? MkInst();
	public abstract Type? ElemType{get;}
	public abstract Type? KeyType{get;}
	public abstract IReadOnlyList<IMemberInfo> Members{get;}

	// 訪問器寫在 Decl（規範約定）；惰性緩存，順序同 Members。
	public IReadOnlyCollection<str> ReadableNames
		=> _readable ??= Members.Where(m => m.CanRead).Select(m => m.CodeName).ToList();
	public IReadOnlyCollection<str> WritableNames
		=> _writable ??= Members.Where(m => m.CanWrite).Select(m => m.CodeName).ToList();
}