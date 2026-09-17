namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// 反射來源的型別元資料：對一個 Type 建立 ITypeInfo。
/// 分類靠介面分析（IDictionary→字典、IEnumerable→集合、基本型別→標量、其餘→物件）；
/// 成員只收公開實例屬性與公開實例字段，順序 = 契約序（TypeInfoSorter：基類在前、
/// 同類內屬性段在字段段前，段內元數據表序）。注意屬性/字段跨 table 沒有統一的
/// 源碼行號，源碼裏屬性字段交錯聲明時，同類內一律「屬性在前、字段在後」。
/// 建構子與 MkInst 實現見 ReflTypeInfo.Impl.cs。
public partial class ReflTypeInfo:TypeInfoBase{
	private readonly Type _type;
	private readonly ETypeKind _kind;
	private readonly IReadOnlyList<IMemberInfo> _members;
	private readonly Type? _elemType;
	private readonly Type? _keyType;
	private readonly Func<obj?>? _mkInstFn;
	private readonly bool _canMkInst;

	/// 對一個型別建立元資料。DAM 註解：反射建立元資料需要 接口/公共屬性/公共字段/
	/// 無參構造函數 的元數據被保留（AOT 剪裁的前提，見 ReflMemberInfo 的說明）。
	// 建構子本體在 ReflTypeInfo.Impl.cs。

	public override Type Type => _type;
	public override ETypeKind Kind => _kind;
	public override IReadOnlyList<IMemberInfo> Members => _members;
	public override Type? ElemType => _elemType;
	public override Type? KeyType => _keyType;
	public override bool CanMkInst => _canMkInst;
}