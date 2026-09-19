namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源的成員元資料：包一個 {nameof(PropertyInfo)} 或 {nameof(FieldInfo)}。]

#Descr[
只收錄公開、實例、非索引器成員
（與 {nameof(JsonTypeInfoSrc)} 的口徑一致；非公開成員按已定決策不進門面）。

例：型別上的 `public i32 Age` 屬性、`public str Note` 字段都會進來；
`private` 屬性、`static` 屬性、`this[i32]` 索引器都不進來，
故兩套來源看到的成員表是同一批。
]

#Descr[
AOT 前提：成員元數據必須被保留
（例如該型別同時掛了 `[JsonSerializable]`，或經 ILLink 模式匹配保留了成員），
否則運行期會因缺元數據拋錯——
這是「兼容 AOT 的反射」的固有前提，由調用方負責，本類不隱藏它。

例：NativeAOT 下若某成員被剪掉，
讀寫委託會在運行期拋異常而不是悄悄返回錯值；
對策是把該型別也掛進源生成上下文，或按 ILLink 規則保留成員。
]

#Descr[
建構子實現見 `ReflMemberInfo.Impl.cs`。
]
""")]
public partial class ReflMemberInfo:MemberInfoBase{
	[Doc($"""
#Sum[包一個公開實例屬性。]

#Params([[要包的屬性]])

#Descr[
例：`new {nameof(ReflMemberInfo)}(typeof(User).GetProperty(nameof(User.Age))!)` 得到 `Age` 的成員元資料，
其 {nameof(IMemberInfo.Member)} 就是那個 {nameof(PropertyInfo)}，
{nameof(IMemberInfo.Json)} 為 null（反射側沒有官方 JSON 本體）。
]
""")]
	internal partial ReflMemberInfo(PropertyInfo Prop);

	[Doc($"""
#Sum[包一個公開實例字段。]

#Params([[要包的字段]])

#Descr[
例：`new {nameof(ReflMemberInfo)}(typeof(User).GetField(nameof(User.Note))!)` 得到 `Note` 的成員元資料，
此時 {nameof(IMemberInfo.MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}，
這一點是反射源比 Json 源強的地方：Json 源對字段只能報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。
]
""")]
	internal partial ReflMemberInfo(FieldInfo Fld);
}