namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(IMemberInfo)} 是型別上單個可訪問成員的元資料，是門面的最小單元。]

#Descr[
與官方 API 的關係（先講清楚官方到底有甚麼，免得再按錯的印象設計）：

+ 官方*沒有*共同的成員基類。
	反射側是 {nameof(System.Reflection.MemberInfo)}
	（{nameof(PropertyInfo)}／{nameof(FieldInfo)} 的基類），
	JsonTypeInfo 側是 {nameof(JsonPropertyInfo)}，
	而 {nameof(JsonPropertyInfo)} 是獨立抽象類、*不繼承* {nameof(System.Reflection.MemberInfo)}
	（已用編譯器核實）。
+ 官方兩側*共有*的是 {nameof(ICustomAttributeProvider)}：
	{nameof(System.Reflection.MemberInfo)} 實現它，
	{nameof(JsonPropertyInfo)} 也提供 {nameof(JsonPropertyInfo.AttributeProvider)} 給它。
	故門面的特性出口就用這一個官方接口，
	不再自造 `Attrs`／`TryGetAttr`。
+ 官方已有的名字與型別一律沿用：
	{nameof(Name)}、{nameof(PropertyType)}、{nameof(DeclaringType)}、
	{nameof(MemberTypes)}、{nameof(Get)}／{nameof(Set)}
	（型別與名字照官方 {nameof(JsonPropertyInfo.Get)}／{nameof(JsonPropertyInfo.Set)}）、
	{nameof(ITypeInfo.CreateObject)}（照官方 {nameof(JsonTypeInfo.CreateObject)}）。
+ 官方沒有的概念才自研，且只有一件：*以字符串為鍵*。
	官方兩側都沒有「按名取成員／按名讀寫」的抽象，
	故本接口補上按名讀寫（{nameof(TryGet)}／{nameof(TrySet)}）與
	型別層的按名查詢（{nameof(ITypeInfo.TryGetMember)}）。
]

#Descr[
同一份成員元資料在兩套來源下的樣子，用同一個屬性成員對照：

+ 反射源：{nameof(Member)} 是 {nameof(PropertyInfo)}，{nameof(Json)} 為 null，
	{nameof(AttributeProvider)} 就是那個 {nameof(PropertyInfo)} 本身，
	{nameof(Get)}／{nameof(Set)} 由 {nameof(PropertyInfo.GetValue)}／{nameof(PropertyInfo.SetValue)} 包成，
	{nameof(MemberType)} 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。
+ Json 源：{nameof(Json)} 是 {nameof(JsonPropertyInfo)}，{nameof(Member)} 為 null，
	{nameof(Get)}／{nameof(Set)} 直接取官方委託（源生成的委託，零反射），
	{nameof(AttributeProvider)} 取官方 {nameof(JsonPropertyInfo.AttributeProvider)}。
+ 兩側的 {nameof(Name)}、{nameof(PropertyType)}、{nameof(DeclaringType)} 語義一致，
	故調用方不需要分支處理。

例：`M.{nameof(TryGet)}(User, out var V)` 在兩套來源下都讀出同一個屬性的值，
`M.{nameof(CanRead)}` 與 `M.{nameof(CanWrite)}` 也都相同。
]

#Descr[
語義約定：

+ {nameof(Name)} 是「鍵」：CsSql 用它當列名，物件與字典互轉用它當字典鍵。
	反射源的 {nameof(Name)} 是 C# 成員名；
	JsonTypeInfo 源的 {nameof(Name)} 是官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Name)}（JSON 名）——
	「用 {nameof(JsonTypeInfoSrc)} 當來源」的成立前提是 JSON 名等於 C# 名
	（由調用方保證命名策略為 null 且無 `[JsonPropertyName]`）。
+ 非公開成員、靜態成員、索引器不進門面（兩套來源一致）。
	例：型別上的 `private` 屬性、`static` 屬性、`this[i32]` 索引器都不會出現在 {nameof(ITypeInfo.Members)} 裏。
+ 讀寫動作的「前置檢查」：
	實例型別不符、不可讀／不可寫時 {nameof(TryGet)}／{nameof(TrySet)} 返回 false；
	值本身型別不符的異常照常拋出，不吞（那是調用方的 bug，不是查詢失敗）。
	例：拿 `B` 的成員去讀 `A` 的實例返回 false；
	拿 `str` 成員寫一個 `i32` 進去則拋 {nameof(ArgumentException)}。
]
""")]
public interface IMemberInfo{
	[Doc($"""
#Sum[官方反射成員本體（{nameof(PropertyInfo)}／{nameof(FieldInfo)}）；{nameof(JsonTypeInfoSrc)} 來源為 null。]

#Descr[
要官方反射能力從這裡拿，例如 {nameof(System.Reflection.MemberInfo.GetCustomAttributes)}、
{nameof(System.Reflection.MemberInfo.IsDefined)}、{nameof(System.Reflection.MemberInfo.ReflectedType)}。

例：`M.{nameof(Member)}!.{nameof(System.Reflection.MemberInfo.Name)}` 在有反射本體時就是 C# 成員名；
`M.{nameof(Member)} is {nameof(PropertyInfo)}` 可判出它是屬性而不是字段。
]
""")]
	global::System.Reflection.MemberInfo? Member{get;}

	[Doc($"""
#Sum[官方 JSON 成員本體（{nameof(JsonPropertyInfo)}）；反射來源為 null。]

#Descr[
要官方序列化能力從這裡拿，例如 {nameof(JsonPropertyInfo.IsRequired)}、
{nameof(JsonPropertyInfo.IsExtensionData)}、{nameof(JsonPropertyInfo.NumberHandling)}。

例：`M.{nameof(Json)}!.{nameof(JsonPropertyInfo.Order)}` 在有 Json 本體時可讀到官方為此成員排的序；
從 {nameof(ReflTypeInfoSrc)} 取得同一個成員時 {nameof(Json)} 是 null，只能走本接口的形狀。
]
""")]
	JsonPropertyInfo? Json{get;}

	[Doc($"""
#Sum[成員的官方元資料種類（官方 {nameof(MemberTypes)}）。]

#Descr[
反射源轉發官方 {nameof(System.Reflection.MemberInfo)}.{nameof(System.Reflection.MemberInfo.MemberType)}；
Json 源恆為 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}——
官方 {nameof(JsonPropertyInfo)} 不暴露 `IsProperty`，
故分不出源生成收進來的字段。

例：反射源包公開屬性得 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}，
包公開字段得 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
同一個字段走 Json 源（`[JsonInclude]` 收進來的）只能得
{nameof(MemberTypes)}.{nameof(MemberTypes.Property)}，這是兩套來源記錄在案的能力差別。

因此要做「是不是字段」的判斷時，別只看這個屬性，
在反射源上另有 {nameof(Member)} 可精確判斷。
]
""")]
	MemberTypes MemberType{get;}

	[Doc($"""
#Sum[成員名，即對外查詢、讀寫時使用的鍵。]

#Descr[
同一個型別元資料內唯一
（同名遮蔽已由 {nameof(TypeInfoSorter)}.{nameof(TypeInfoSorter.SortEtDedup)} 去重，
見 {nameof(ITypeInfo.Members)}）。

反射源 = 官方 {nameof(System.Reflection.MemberInfo.Name)}；
JsonTypeInfo 源 = 官方 {nameof(JsonPropertyInfo.Name)}。

例：{nameof(ITypeInfo.GetMember)}("Age") 用的 "Age" 就是這個屬性的值；
它是 CsSql 的列名、也是物件與字典互轉時的字典鍵，故拼錯會直接查不到成員。
]
""")]
	str Name{get;}

	[Doc($"""
#Sum[成員的宣告型別。]

#Descr[
即屬性的 {nameof(PropertyInfo.PropertyType)}／字段的 {nameof(FieldInfo.FieldType)}，
兩者在官方 {nameof(JsonPropertyInfo.PropertyType)} 上是同一個概念。

例：`i32 Age` 屬性的 {nameof(PropertyType)} 是 `typeof(i32)`；
`{nameof(List<int>)} Tags` 的是 `typeof({nameof(List<int>)})`，
即集合成員給的是集合型別本身，不是它的元素型別（元素型別要看 {nameof(ITypeInfo.ElementType)}）。
]
""")]
	Type PropertyType{get;}

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
反射源轉發官方 {nameof(System.Reflection.MemberInfo)}.{nameof(System.Reflection.MemberInfo.DeclaringType)}；
Json 源轉發官方 {nameof(JsonPropertyInfo.DeclaringType)}。

繼承成員的 {nameof(DeclaringType)} 是基類，與實例的執行期型別不同。

例：子類繼承了基類的成員，從子類的元資料取這個繼承成員時，
{nameof(DeclaringType)} 是基類的 {nameof(Type)}，
而 {nameof(ITypeInfo)}.{nameof(ITypeInfo.Type)} 是子類的 {nameof(Type)}，兩者不相等；
要拿實例的執行期型別應該查 {nameof(ITypeInfo.Type)}，不是本屬性。
]
""")]
	Type? DeclaringType{get;}

	[Doc($"""
#Sum[本成員可讀（可作為字典值來源、可被 {nameof(TryGet)} 讀取）。]

#Descr[
判據是官方 {nameof(Get)} 是否為 null
（官方 {nameof(JsonPropertyInfo)} 就是這樣表示可讀）；
反射側同樣以「有沒有公開訪問器」得出 {nameof(Get)}，兩套來源口徑一致。

例：公開讀寫屬性為 true；
只寫屬性（`set` 公開、`get` 為私有）為 false，
此時 {nameof(Get)} 是 null、{nameof(TryGet)} 返回 false；
公開字段為 true，但 `readonly` 字段在反射源上仍是 true（可讀），
只是 {nameof(CanWrite)} 為 false。
]
""")]
	bool CanRead{get;}

	[Doc($"""
#Sum[本成員可寫（可被 {nameof(TrySet)} 寫入）。]

#Descr[
判據同 {nameof(CanRead)}，取官方 {nameof(Set)} 是否為 null。

例：只讀屬性（有 `get`、無公開 `set`）為 false，此時 {nameof(Set)} 是 null；
只寫屬性為 true；
`const` 字段在反射源上為 false，且 {nameof(CanRead)} 也為 false（常量根本取不到值）。
]
""")]
	bool CanWrite{get;}

	[Doc($"""
#Sum[官方讀值委託，型別與名字照官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)}。]

#Descr[
反射側由 {nameof(PropertyInfo.GetValue)}／{nameof(FieldInfo.GetValue)} 建成同一形狀。

不可讀為 null；委託只做取值，前置檢查由 {nameof(TryGet)} 承擔。

例：`M.{nameof(Get)}!(User)` 直接讀出值，不做型別檢查，
傳錯實例會由底層拋異常；
要安全版本就用 `M.{nameof(TryGet)}(User, out var V)`。

要自己批量讀值時用這個委託比反復調 {nameof(TryGet)} 少一層檢查，代價是得自己保證實例型別。
]
""")]
	Func<obj, obj?>? Get{get;}

	[Doc($"""
#Sum[官方寫值委託，型別與名字照官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)}。]

#Descr[
不可寫為 null。

例：`M.{nameof(Set)}!(User, 31)` 直接把值寫進實例；
`M.{nameof(Set)}` 為 null 就說明這個成員不可寫，調用方應據此判斷而不是硬調。
]
""")]
	Action<obj, obj?>? Set{get;}

	[Doc($"""
#Sum[官方特性提供者（兩側共有的那個官方接口）。]

#Descr[
反射源即成員自身；
Json 源取官方 {nameof(JsonPropertyInfo.AttributeProvider)}。

取值用 {nameof(IMemberInfoExtn)}.{nameof(IMemberInfoExtn.GetCustomAttribute)}（官方風格的便利方法），
或直接用官方 {nameof(ICustomAttributeProvider)}.{nameof(ICustomAttributeProvider.GetCustomAttributes)}。

例：`M.{nameof(IMemberInfoExtn.GetCustomAttribute)}<MyAttr>()` 取不到就返回 null，
取得到就返回那個特性實例；
源生成下這個出口同樣有效（實測可取到特性），故 AOT 下不必另走反射。
]
""")]
	ICustomAttributeProvider? AttributeProvider{get;}

	[Doc($"""
#Sum[讀取實例上的本成員。]

#Params([[實例；null 或型別不符時返回 false], [讀出的值；失敗時為 default]])

#Rtn[實例型別不符或本成員不可讀時返回 false]

#Descr[
例：`Info.{nameof(ITypeInfo.GetMember)}("Age").{nameof(TryGet)}(User, out var V)` 命中，
`V` 是 boxed 的 `i32`，要用 `(i32)V!` 拆箱；
傳 null 或傳另一種型別的實例返回 false 且 `V` 為 default；
讀只寫成員同樣返回 false，此時應先查 {nameof(CanRead)} 免得白跑一趟。
]
""")]
	bool TryGet(obj? O, out obj? R);

	[Doc($"""
#Sum[寫入實例上的本成員。]

#Params([[實例；null 或型別不符時返回 false], [要寫入的值]])

#Rtn[實例型別不符或本成員不可寫時返回 false]

#Descr[
例：`Info.{nameof(ITypeInfo.GetMember)}("Age").{nameof(TrySet)}(User, 31)` 命中，
之後 `User.Age` 就是 31；
寫只讀成員返回 false 且不改動實例（不拋異常，故可用於「能寫就寫」的批量回填）；
值型別不符（拿 `str` 當 `i32` 寫）不返回 false，而是照常拋異常，
因為那是調用方的 bug，不是「這個成員不可寫」。
]
""")]
	bool TrySet(obj? O, obj? V);
}