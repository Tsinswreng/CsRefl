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
實測（`PoUser` 的 `Age`）：兩套來源的 {nameof(Name)} 都是 "Age"、
{nameof(IMemberInfo.PropertyType)} 都是 `typeof(i32)`、
{nameof(IMemberInfo.DeclaringType)} 都是 `typeof(PoUser)`、
{nameof(IMemberInfo.CanRead)} 與 {nameof(IMemberInfo.CanWrite)} 都是 true；
差別只在 {nameof(Member)} 與 {nameof(Json)} 哪個非 null：
反射源 {nameof(Member)} 是 {nameof(PropertyInfo)}、{nameof(Json)} 為 null，
Json 源相反。
字段成員（`Note`）在反射源報 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}，
在 Json 源只能報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。
]

#Descr[
建構子實現見 `JsonMemberInfo.Impl.cs`。
]
""")]
public partial class JsonMemberInfo:MemberInfoBase{
	[Doc($"""
#Sum[包一個 {nameof(JsonPropertyInfo)}。]

#Params([[Prop, 要包的官方 JSON 成員元資料]])

#Descr[
實測（`PoUser`）：`Info.{nameof(JsonTypeInfo.Properties)}[0]` 就是 `Id` 這個成員，
包出來後 {nameof(Name)} 是 "Id"、{nameof(IMemberInfo.PropertyType)} 是 `typeof(i64)`、
{nameof(IMemberInfo.DeclaringType)} 是 `typeof(PoUserBase)`；
其 {nameof(Get)}／{nameof(Set)} 直接就是官方源生成的委託，
故在 NativeAOT 下讀寫也不走反射（本庫的 AOT 測試即跑在這條路徑上）。
]
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Prop);
}