namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfo 來源的型別元資料：包一個非泛型 JsonTypeInfo。
/// Kind 直接映射 JsonTypeInfoKind（None→標量）；成員沿用 JsonTypeInfo.Properties
/// 的既有序（Order 屬性升序，同序按宣告序）。
/// 建構子與 MkInst 實現見 JsonTypeInfoInfo.Impl.cs。
public partial class JsonTypeInfoInfo:TypeInfoBase{
	private readonly JsonTypeInfo _json;
	private readonly Type _type;
	private readonly ETypeKind _kind;
	private readonly IReadOnlyList<IMemberInfo> _members;
	private readonly Type? _elemType;
	private readonly Type? _keyType;

	public override Type Type => _type;
	public override ETypeKind Kind => _kind;
	public override IReadOnlyList<IMemberInfo> Members => _members;
	public override Type? ElemType => _elemType;
	public override Type? KeyType => _keyType;
	/// 是否可建實例：Kind 為 Object 且存在無參工廠（CreateObject 不為 null）。
	public override bool CanMkInst => _json.Kind == JsonTypeInfoKind.Object && _json.CreateObject is not null;
}