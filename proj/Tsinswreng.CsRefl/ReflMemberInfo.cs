namespace Tsinswreng.CsRefl;

using System.Reflection;

/// 反射來源的成員元資料：包一個 PropertyInfo 或 FieldInfo。
/// 只收錄公開、實例、非索引器成員（與 JsonTypeInfo 來源的口徑一致；
/// 非公開成員按已定決策不進門面）。
///
/// AOT 前提：成員元數據必須被保留（例如該型別同時掛了 [JsonSerializable]，
/// 或經 ILLink 模式匹配保留了成員），否則運行期會因缺元數據拋錯——這是
/// 「兼容 AOT 的反射」的固有前提，由調用方負責，本類不隱藏它。
///
/// Order 用收集序而非 MetadataToken：NativeAOT 的反射元數據（NativeFormat）
/// 不提供 MetadataToken，取它會拋 InvalidOperationException。
/// 建構子內部使用（見 ReflTypeInfo.CollectMembers）。
public partial class ReflMemberInfo:MemberInfoBase{
	private readonly str _codeName;
	private readonly str? _jsonName;
	private readonly EMemberKind _kind;
	private readonly Type _declaredType;
	private readonly Type _declaringType;
	private readonly bool _canRead;
	private readonly bool _canWrite;
	private readonly i32 _order;
	private readonly IReadOnlyList<Attribute> _attrs;

	public override str CodeName => _codeName;
	public override str? JsonName => _jsonName;
	public override EMemberKind Kind => _kind;
	public override Type DeclaredType => _declaredType;
	public override Type DeclaringType => _declaringType;
	public override bool CanRead => _canRead;
	public override bool CanWrite => _canWrite;
	public override i32 Order => _order;
	public override IReadOnlyList<Attribute> Attrs => _attrs;
}