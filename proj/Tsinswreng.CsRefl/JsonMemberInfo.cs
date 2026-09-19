namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[JsonTypeInfo 來源的成員元資料：包一個非泛型 {nameof(JsonPropertyInfo)}。]

#Descr[
官方事實（實測，JIT 與 win-x64 NativeAOT 皆同）：

+ 官方 {nameof(JsonPropertyInfo)} *不繼承* {nameof(System.Reflection.MemberInfo)}——
	它是獨立抽象類，官方這兩個體系沒有共同基類。
	故門面只把官方成員對象交給 {nameof(Json)} 出口，{nameof(Member)} 出口留 null。
+ 官方提供 {nameof(JsonPropertyInfo.AttributeProvider)}，且源生成下*確實能取到特性*
	（實測標在成員上的自訂特性取回 1 個），所以在 AOT 下同樣可用。
	這一點推翻了本包早期「源生成下特性恒空、統一不支持」的判斷，已改為照官方取。
+ 官方不暴露 `IsProperty`，故無法分辨源生成收進來的字段（`[JsonInclude]`）；
	{nameof(MemberType)} 只能一律報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}——
	這是兩套來源記錄在案的能力差別。
]

#Descr[
例：同一個成員分別走兩套來源時，
{nameof(Name)}、{nameof(IMemberInfo.PropertyType)}、{nameof(IMemberInfo.DeclaringType)}
在兩邊取到的值相同，
差別只在 {nameof(Member)} 與 {nameof(Json)} 哪個非 null，
以及字段成員在 Json 源上報不出 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}。
]

#Descr[
建構子實現見 `JsonMemberInfo.Impl.cs`。
]
""")]
public partial class JsonMemberInfo:MemberInfoBase{
	[Doc($"""
#Sum[包一個 {nameof(JsonPropertyInfo)}。]

#Params([[要包的官方 JSON 成員元資料]])

#Descr[
例：`new {nameof(JsonMemberInfo)}(Info.{nameof(JsonTypeInfo.Properties)}[0])` 得到第一個成員，
其 {nameof(Get)}／{nameof(Set)} 直接就是官方源生成的委託，
故在 NativeAOT 下讀寫也不走反射。
]
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Prop);
}