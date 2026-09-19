namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[反射來源的成員元資料：包一個 {nameof(PropertyInfo)} 或 {nameof(FieldInfo)}。]

#Descr[
只收錄公開、實例、非索引器成員
（與 {nameof(JsonTypeInfoSrc)} 的口徑一致；非公開成員按已定決策不進門面）。

實測：型別上的 `public i32 Age` 屬性、`public str Note` 字段都會進來；
`private` 屬性、`static` 屬性、`this[i32]` 索引器都不進來，
故兩套來源看到的成員表是同一批。

實測：查 `PoUser` 得到 11 個成員，其中 `Age` 是屬性、`Note` 是字段；
同型別上另加的 `StaticNote`（靜態）、`Hidden`（私有）、`this[i32]`（索引器）
實測都不在成員表內。
]

#Descr[
AOT 前提：成員元數據必須被保留
（例如該型別同時掛了 `[JsonSerializable]`，或經 ILLink 模式匹配保留了成員），
否則運行期會因缺元數據拋錯——
這是「兼容 AOT 的反射」的固有前提，由調用方負責，本類不隱藏它。

實測：本庫的 NativeAOT（win-x64）測試裏 11 個成員全部可取、讀寫正常；
若某成員被剪掉，
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

#Params([[Prop, 要包的屬性]])

#Descr[
實測：`new {nameof(ReflMemberInfo)}(typeof(PoUser).GetProperty(nameof(PoUser.Age))!)` 得到 `Age` 的成員元資料，
其 {nameof(IMemberInfo.Member)} 就是那個 {nameof(PropertyInfo)}，
{nameof(IMemberInfo.Json)} 為 null（反射側沒有官方 JSON 本體）。

實測（`nameof(PoUser.Age)`）：{nameof(IMemberInfo.Name)} 是 "Age"、
{nameof(IMemberInfo.PropertyType)} 是 `typeof(i32)`、
{nameof(IMemberInfo.DeclaringType)} 是 `typeof(PoUser)`、
{nameof(IMemberInfo.MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}、
{nameof(IMemberInfo.CanRead)} 與 {nameof(IMemberInfo.CanWrite)} 都是 true。
]
""")]
	internal partial ReflMemberInfo(PropertyInfo Prop);

	[Doc($"""
#Sum[包一個公開實例字段。]

#Params([[Fld, 要包的字段]])

#Descr[
實測：`new {nameof(ReflMemberInfo)}(typeof(PoUser).GetField(nameof(PoUser.Note))!)` 得到 `Note` 的成員元資料，
此時 {nameof(IMemberInfo.MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}，
這一點是反射源比 Json 源強的地方：Json 源對字段只能報 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。

實測（`nameof(PoUser.Note)`）：{nameof(IMemberInfo.Name)} 是 "Note"、
{nameof(IMemberInfo.PropertyType)} 是 `typeof(str)`、
{nameof(IMemberInfo.MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}
（同一成員在 Json 源上只能得到 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}）。
]
""")]
	internal partial ReflMemberInfo(FieldInfo Fld);

	// ---- 私有輔助（實現見 ReflMemberInfo.Impl.cs）----

	[Doc($"""
#Sum[按屬性可讀性建讀值委託。]

#Params([[Prop, 要包的屬性]])

#Rtn[讀值委託；不可讀返回 null]

#Descr[
判據是「有沒有公開 get」（{nameof(PropertyInfo.GetGetMethod)} 預設只認公開訪問器），
不是官方 {nameof(PropertyInfo.CanRead)}：
屬性帶私有 get 時官方 {nameof(PropertyInfo.CanRead)} 仍為 true，
用它會把只寫屬性誤判成可讀（門面承諾的是公開成員的讀寫能力）。

實測：`PoUser.Age`（`get` 與 `set` 都公開）返回委託，調它讀出物件上的值；
`PoUser.Token`（`get` 私有）返回 null，
此時 {nameof(IMemberInfo.CanRead)} 也是 false，{nameof(IMemberInfo.Get)} 也是 null，三處口徑一致。
]
""")]
	private static partial Func<obj, obj?>? BuildGet(PropertyInfo Prop);

	[Doc($"""
#Sum[按屬性可寫性建寫值委託。]

#Params([[Prop, 要包的屬性]])

#Rtn[寫值委託；不可寫返回 null]

#Descr[
判據是「有沒有公開 set」，理由同 {nameof(BuildGet)}。

實測：`PoUser.Age` 返回委託，調它把 31 寫進物件；
`PoUser.Secret`（只有 `get`）返回 null，故 {nameof(IMemberInfo.Set)} 為 null、
{nameof(IMemberInfo.TrySet)} 返回 false。
]
""")]
	private static partial Action<obj, obj?>? BuildSet(PropertyInfo Prop);

	[Doc($"""
#Sum[建字段讀值委託。]

#Params([[Fld, 要包的字段]])

#Rtn[讀值委託；常量（無從取值）返回 null]

#Descr[
實測：`PoUser.Note`（`public str Note`）返回委託，調它讀出該字段的值；
`public const i32 Max = 100;` 那種常量返回 null（{nameof(FieldInfo.IsLiteral)} 為 true），
故常量的 {nameof(IMemberInfo.CanRead)} 與 {nameof(IMemberInfo.CanWrite)} 都是 false。
]
""")]
	private static partial Func<obj, obj?>? BuildFieldGet(FieldInfo Fld);

	[Doc($"""
#Sum[建字段寫值委託。]

#Params([[Fld, 要包的字段]])

#Rtn[寫值委託]

#Descr[
實測：`PoUser.Note` 返回委託，調它把值寫進該字段；
`readonly` 字段也返回委託，但可寫性由建構子另行判為 false
（見 {nameof(ReflMemberInfo)} 的字段建構子），故這裡不重複判 {nameof(FieldInfo.IsInitOnly)}。
]
""")]
	private static partial Action<obj, obj?>? BuildFieldSet(FieldInfo Fld);
}