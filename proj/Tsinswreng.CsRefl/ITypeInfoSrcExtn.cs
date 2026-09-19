namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[操作層：在 {nameof(ITypeInfoSrc)} 之上的按名讀寫與物件與淺字典互轉。]

#Descr[
這裡是調用方（CsSql、Ngan.Dict 業務）真正接觸的面：
來源只負責「查型別」，
本類負責把查詢結果拼成好用的一次調用。

例：不需要先 `TryGetInfo` 再 `TryGetMember` 再 `TryGet` 三步，
直接 `Src.{nameof(TryGet)}(typeof(User), "Age", User, out var V)` 一次拿到值。
]

#Descr[
參數名的選擇：
成員名參數一律叫 `Name`，
與官方 {nameof(System.Reflection.MemberInfo.Name)}／{nameof(System.Text.Json.Serialization.Metadata.JsonPropertyInfo.Name)} 同名
（門面的「鍵」就是官方那個名字，沒有另造概念）。

實現見 `ITypeInfoSrcExtn.Impl.cs`
（傳統擴展方法語法支持 `partial` 拆分）。
]
""")]
public static partial class ITypeInfoSrcExtn{
	[Doc($"""
#Sum[運行期型別的 DAM 擔保。]

#Params([[運行期型別，通常來自 `obj.GetType()`]])

#Rtn[原樣返回入參]

#Descr[
動態 `GetType()` 在分析器眼裏不攜帶 DAM 信息，
但本包在此路徑上對 `T` 的用法只有 {nameof(Type.IsInstanceOfType)} 與反射建元資料，
缺元數據時會在反射建元資料處自然拋錯，不會悄悄剪錯，
故此處顯式擔保成員元數據需求。

例：`{nameof(ToInstDict)}(User)` 內部就是先 `{nameof(RuntimeType)}(User.GetType())`，
再拿這個 {nameof(Type)} 去查來源；調用方不必自己處理 DAM。
]
""")]
	[UnconditionalSuppressMessage("Trimming", "IL2068",
		Justification = "運行期型別（O.GetType()）本質無法靜態攜帶 DAM 信息；本包對它的用法只有 IsInstanceOfType 與反射建元資料，缺元數據時在反射建元資料處自然拋錯，不會悄悄剪錯。")]
	[return: DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)]
	private static partial Type RuntimeType(Type T);

	[Doc($"""
#Sum[取成員。]

#Params([[來源], [要查的型別], [成員名]])

#Rtn[取到的成員元資料]

#Descr[
型別未註冊或成員不存在都拋 {nameof(KeyNotFoundException)}（訊息含線索）。

`Type` 的 DAM 註解：
後端會把型別交給來源查元資料（反射來源需要成員元數據）。

例：`Src.{nameof(GetMember)}(typeof(User), "Age")` 返回 `Age` 的 {nameof(IMemberInfo)}；
成員名拼錯成 `"NoSuch"` 則拋，訊息裏同時有型別全名與成員名，可直接拿去排查；
型別沒註冊到這個來源時拋的也是 {nameof(KeyNotFoundException)}，但訊息指出的是型別未註冊。
]
""")]
	public static partial IMemberInfo GetMember(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name
	);

	[Doc($"""
#Sum[取成員的 Try 版。]

#Params([[來源], [要查的型別], [成員名], [取到的成員；失敗時為 null]])

#Rtn[型別未註冊或成員不存在都返回 false]

#Descr[
例：先 `Src.{nameof(TryGetMember)}(T, "Age", out var M)` 判真假，
命中才用 `M`，不命中就當這條欄位不存在；
入參 `z` 或 `Type` 為 null 同樣返回 false 而不拋，故適合接外部傳來的名字。

不確定名字是否在型別上時用這個；確定必須成功時用 {nameof(GetMember)}。
]
""")]
	public static partial bool TryGetMember(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		[NotNullWhen(true)] out IMemberInfo? M
	);

	[Doc($"""
#Sum[按名讀值。]

#Params([[來源], [物件的型別], [成員名], [實例], [讀出的值]])

#Rtn[型別、成員、實例、可讀性任一不滿足返回 false]

#Descr[
例：`Src.{nameof(TryGet)}(typeof(User), "Age", User, out var V)` 命中，
`V` 是 boxed 的 `i32`；讀只讀成員照樣命中（可讀），
讀只寫成員返回 false；傳錯型別的實例返回 false。

三步（查型別、查成員、讀值）合一，失敗一律用 false 表示，
故適合批量回填、按外部欄位名取值這類「能取就取」的場景。
]
""")]
	public static partial bool TryGet(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		obj? O,
		out obj? R
	);

	[Doc($"""
#Sum[按名寫值。]

#Params([[來源], [物件的型別], [成員名], [實例], [要寫入的值]])

#Rtn[型別、成員、實例、可寫性任一不滿足返回 false]

#Descr[
例：`Src.{nameof(TrySet)}(typeof(User), "Age", User, 31)` 命中，之後 `User.Age` 是 31；
寫只讀成員返回 false 且不動實例；
值型別不符（拿 `str` 當 `i32` 寫）不返回 false，而是照常拋異常。

區分「不可寫」與「寫錯型別」是刻意的：
前者是型別設計決定的、可以無視；後者是調用方的 bug、必須爆出來。
]
""")]
	public static partial bool TrySet(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		obj? O,
		obj? V
	);

	[Doc($"""
#Sum[建淺字典視圖，型別取 `O.GetType()`。]

#Params([[來源], [目標物件]])

#Rtn[該物件的淺字典視圖]

#Descr[
型別取執行期型別，
修掉舊 Srefl 用 `typeof(T)` 查不到介面與基類成員的隱患。

`O` 為 null 拋 {nameof(ArgumentNullException)}。

例：`Src.{nameof(ToInstDict)}(User)` 得到 {nameof(IInstDict)}
（視圖的鍵是運行期型別上可讀可寫的成員）；
若 `User` 的靜態型別是基類而運行期是子類，
這裡查的是子類的元資料，故子類新增的成員也在視圖裏。
]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O);

	[Doc($"""
#Sum[建淺字典視圖，型別可由調用方顯式給。]

#Params([[來源], [目標物件], [物件的型別；為 null 時退到 `O.GetType()`]])

#Rtn[該物件的淺字典視圖]

#Descr[
例：傳 `typeof(Base)` 而物件其實是子類，
則視圖只認基類宣告的成員（子類新增的成員不在鍵表裏，也讀寫不了），
這就是「只要基類那一部分」時該用的形狀。
]
""")]
	public static partial IInstDict ToInstDict(
		this ITypeInfoSrc z,
		obj? O,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type? Type
	);

	[Doc($"""
#Sum[把字典寫回物件（型別取 `O.GetType()`）。]

#Params([[來源], [目標物件], [要寫回的字典]])

#Descr[
按名逐鍵 {nameof(TrySet)}，只讀成員跳過；
鍵不在型別上 → {nameof(KeyNotFoundException)}（訊息同時列可寫名與可讀名，便於對照）；
值型別不符 → {nameof(InvalidOperationException)}。

例：外部傳來一個字典，裏面有 `Age` 一個正常值、還有一個只讀成員的鍵，
`Age` 正常寫入，`Secret` 因只讀被靜默跳過（不算錯，這是型別決定的）；
字典裏多一個 `"NoSuch"` 就不是跳過了，直接拋，因為那是鍵名對不上。

要「只寫認得的鍵、其餘不管」的寬鬆語義，就先自己過濾字典再用；
本方法選擇嚴格，是因為寫回物件通常發生在反序列化路徑上，靜默丟鍵比拋異常更難查。
]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict);

	[Doc($"""
#Sum[把字典寫回物件，型別由調用方顯式給。]

#Params([[來源], [目標物件], [要寫回的字典], [物件的型別；為 null 時退到 `O.GetType()`]])

#Descr[
例：只想把字典裏的基類欄位寫進去時傳 `typeof(Base)`，
子類新增的鍵就會因為「不在基類成員表上」而拋 {nameof(KeyNotFoundException)}，
故這種用法通常要先把字典裁剪到基類欄位。
]
""")]
	public static partial void AssignFromDict(
		this ITypeInfoSrc z,
		obj? O,
		IReadOnlyDictionary<str, obj?> Dict,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type? Type
	);
}