namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfo 來源的成員元資料：包一個非泛型 JsonPropertyInfo。
/// 建構子見 JsonMemberInfo.Impl.cs。
///
/// 兩點記錄在案的能力差別（與反射來源不同）：
/// - Kind 恆為 Property：非泛型 JsonPropertyInfo 不暴露 IsProperty，無法分辨
///   源生成收進來的字段（[JsonInclude]），統一按 Property 處理；
/// - Attrs 恆為空表、TryGetAttr 恆 false：源生成模式下 AttributeProvider 走
///   反射路徑（typeof(X).GetProperty(...)），AOT 下不可靠，統一按「不支持」。
public partial class JsonMemberInfo:MemberInfoBase{
	public override str CodeName {get;}
	public override str? JsonName {get;}
	public override EMemberKind Kind => EMemberKind.Property;
	public override Type DeclaredType {get;}
	public override Type DeclaringType {get;}
	public override bool CanRead {get;}
	public override bool CanWrite {get;}
	public override i32 Order {get;}
	public override IReadOnlyList<Attribute> Attrs => [];
}