namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfo 來源的成員元資料：包一個非泛型 JsonPropertyInfo。
/// 建構子實現見 JsonMemberInfo.Impl.cs。
///
/// 官方事實（2026-09-17 實測，JIT 與 win-x64 NativeAOT 皆同）：
/// - 官方 JsonPropertyInfo 本身即 MemberInfo 子類，故 DeclaringType／MemberType
///   都直接轉發它，門面不需要自己另存一份；
/// - 官方提供 AttributeProvider，且源生成下**確實能取到特性**
///   （實測 Level 上的 MyDemoAttr 取回 1 個），所以在 AOT 下同樣可用；
///   —— 這一點推翻了本包早期「源生成下 Attrs 恒空、統一不支持」的判斷，已改為照官方取。
/// - 官方不暴露 IsProperty，故無法分辨源生成收進來的字段（[JsonInclude]）；
///   成員種類一律看官方 Member 的實際型別（字段會給出 RtFieldInfo）。
public partial class JsonMemberInfo:MemberInfoBase{
	/// 包一個 JsonPropertyInfo。
	public partial JsonMemberInfo(JsonPropertyInfo Prop);
}