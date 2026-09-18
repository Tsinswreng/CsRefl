namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc("""
#Sum[多來源合成：按構造順序逐個查詢，第一個答「已知」的來源勝出。]

#Descr[
典型用法：
JsonTypeInfo 來源為主（AOT 主路徑、讀寫是委託），
反射來源兜底（覆蓋沒掛 `[JsonSerializable]` 的型別）。

來源各自內部緩存，本類不重建緩存。
]

#Descr[
建構子 / `TryGetInfo` / 列舉快照的實現見 `MergedTypeInfoSrc.Impl.cs`。
]
""")]
public partial class MergedTypeInfoSrc:ITypeInfoSrc{
	[Doc("""
#Sum[來源清單，順序即優先級。]

#Descr[
建構子做防禦拷貝，構造後不受外部數組改動影響。
]
""")]
	private readonly IReadOnlyList<ITypeInfoSrc> _sources;

	[Doc("""
#Sum[全部來源都支持列舉才返回並集快照，否則返回 null。]

#Descr[
與「來源可不同構」的設計一致。

快照按訪問現算：來源本身可能在建構後繼續註冊，緩存反而會給出過期答案。
]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	[Doc("""
#Sum[按優先級順序給出來源。]

#Params([[來源，順序即優先級；至少要一個]])
""")]
	public partial MergedTypeInfoSrc(params ITypeInfoSrc[] Sources);

	[Doc("""
#Sum[第一個答「已知」的來源勝出；全部答「未知」返回 false。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ITypeInfo? Info
	);
}