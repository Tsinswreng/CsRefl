namespace Tsinswreng.CsRefl;

/// 多來源合成：按構造順序逐個查詢，第一個答「已知」的來源勝出。
/// 典型用法：JsonTypeInfo 來源為主（AOT 主路徑、讀寫是委託），反射來源兜底
/// （覆蓋沒掛 [JsonSerializable] 的型別）。來源各自內部緩存，本類不重建緩存。
public partial class MergedTypeInfoSrc:ITypeInfoSrc{
	/// 來源清單，順序即優先級。
	private readonly IReadOnlyList<ITypeInfoSrc> _sources;

	/// 全部來源都支持列舉才返回並集快照，否則返回 null（與「來源可不同構」的設計一致）。
	public IReadOnlyCollection<Type>? RegisteredTypes => SnapshotTypes();

	// 建構子 / TryGetInfo / SnapshotTypes 的實現見 MergedTypeInfoSrc.Impl.cs。
}