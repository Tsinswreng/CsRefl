namespace Tsinswreng.CsRefl;

/// 可寫的型別元資料註冊表：手動塞 ITypeInfo、支持列舉，供「反射/JSON 都覆蓋不到」
/// 的場合（如 CsSql 內部的 SchemaHistory，手寫 5 個成員的元資料即可）。
/// 線程安全：內部用鎖保護讀寫；初始化期填完之後當只讀用也安全。
public partial class TypeInfoReg:ITypeInfoReg{
	/// 讀寫鎖。
	private readonly object _sync = new();
	/// 型別 → 元資料表。
	private readonly Dictionary<Type, ITypeInfo> _map = new();

	/// 列舉已註冊型別（快照）。
	public IReadOnlyCollection<Type>? RegisteredTypes => SnapshotTypes();

	// TryGetInfo / Add / Remove / SnapshotTypes 的實現見 TypeInfoReg.Impl.cs。
}