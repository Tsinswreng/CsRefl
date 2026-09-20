namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[操作層：在 {nameof(ITypeInfoSrc)} 之上的按名讀寫與物件與淺字典互轉。]

#Descr[
這裡是調用方（CsSql、Ngan.Dict 業務）真正接觸的面：
來源只負責「查型別」，
本類負責把查詢結果拼成好用的一次調用。

實測：不必先 `TryGetInfo` 再 `TryGetMember` 再 `TryGet` 三步，
直接 `Src.{nameof(TryGet)}(typeof(PoUser), "Age", User, out var V)` 一次拿到值，
當 `User.Age` 是 30 時 `V` 是 boxed 的 `i32` 30；
同樣的三步合寫在 {nameof(ToInstDict)} 與 {nameof(AssignFromDict)} 上，
故調用方（CsSql、Ngan.Dict）不必自己碰來源與成員兩層。
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
#Sum[查型別的元資料；查不到就拋。]

#Params([[z, 來源], [Type, 要查的型別]])

#Rtn[取到的型別元資料]

#Descr[
與 {nameof(ITypeInfoSrc.TryGetInfo)} 成對，正如 {nameof(GetMember)} 之於 {nameof(TryGetMember)}：
型別沒註冊到這個來源時拋 {nameof(KeyNotFoundException)}，訊息指出是哪個型別與哪個來源。

實測：`Src.{nameof(GetInfo)}(typeof(PoUser))` 返回的 {nameof(ITypeInfo.Type)} 是 `typeof(PoUser)`；
對只認已註冊型別的 {nameof(JsonTypeInfoSrc)} 查 `typeof(PoNoCtor)` 拋 {nameof(KeyNotFoundException)}。

確定型別一定查得到時用本方法，省掉調用方的 `if(!TryGetInfo(...))` 樣板。
]
""")]
	public static partial ITypeInfo GetInfo(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type
	);

	[Doc($"""
#Sum[運行期型別的 DAM 擔保。]

#Params([[T, 運行期型別，通常來自 `obj.GetType()`]])

#Rtn[原樣返回入參]

#Descr[
動態 `GetType()` 在分析器眼裏不攜帶 DAM 信息，
但本包在此路徑上對 `T` 的用法只有 {nameof(Type.IsInstanceOfType)} 與反射建元資料，
缺元數據時會在反射建元資料處自然拋錯，不會悄悄剪錯，
故此處顯式擔保成員元數據需求。

實測：`{nameof(ToInstDict)}(User)` 內部就是先 `{nameof(RuntimeType)}(User.GetType())`，
再拿這個 {nameof(Type)} 去查來源（實測得到的是 `typeof(PoUser)`）；
調用方不必自己處理 DAM，剪裁分析器也不會因此報警。
]
""")]
	[UnconditionalSuppressMessage("Trimming", "IL2068",
		Justification = "運行期型別（O.GetType()）本質無法靜態攜帶 DAM 信息；本包對它的用法只有 IsInstanceOfType 與反射建元資料，缺元數據時在反射建元資料處自然拋錯，不會悄悄剪錯。")]
	[return: DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)]
	private static partial Type RuntimeType(Type T);

	[Doc($"""
#Sum[取成員。]

#Params([[z, 來源], [Type, 要查的型別], [Name, 成員名]])

#Rtn[取到的成員元資料]

#Descr[
型別未註冊或成員不存在都拋 {nameof(KeyNotFoundException)}（訊息含線索）。

`Type` 的 DAM 註解：
後端會把型別交給來源查元資料（反射來源需要成員元數據）。

實測：`Src.{nameof(GetMember)}(typeof(PoUser), "Age")` 返回的
{nameof(IMemberInfo.PropertyType)} 是 `typeof(i32)`、{nameof(IMemberInfo.DeclaringType)} 是 `typeof(PoUser)`；
成員名拼錯成 `"NoSuch"` 拋 {nameof(KeyNotFoundException)}，訊息含成員名、可直接拿去排查；
型別沒註冊到這個來源（如 {nameof(JsonTypeInfoSrc)} 查 `typeof(PoNoCtor)`）拋的也是
{nameof(KeyNotFoundException)}，但訊息指出的是型別未註冊，兩種錯分得開。
]
""")]
	public static partial IMemberInfo GetMember(
		this ITypeInfoSrc z,
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		str Name
	);

	[Doc($"""
#Sum[取成員的 Try 版。]

#Params([[z, 來源], [Type, 要查的型別], [Name, 成員名], [M, 取到的成員；失敗時為 null]])

#Rtn[型別未註冊或成員不存在都返回 false]

#Descr[
實測：`Src.{nameof(TryGetMember)}(typeof(PoUser), "Age", out var M)` 返回 true 且 `M` 非 null；
名字換成 `"NoSuch"`、或型別換成未註冊的 `typeof(PoNoCtor)`、
或來源與型別傳 null，都返回 false 且 `M` 為 null、一律不拋，
故適合接外部傳來的名字。

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

#Params([[z, 來源], [Type, 物件的型別], [Name, 成員名], [O, 實例], [R, 讀出的值]])

#Rtn[型別、成員、實例、可讀性任一不滿足返回 false]

#Descr[
實測（`PoUser`，`Age = 30`、`Secret = "s"`）：`Src.{nameof(TryGet)}(typeof(PoUser), "Age", User, out var V)` 返回 true 且 `V` 是 boxed 的 `i32` 30；
`"Secret"`（只讀）也返回 true 且 `V` 是 "s"；
`"Token"`（只寫）返回 false；傳 `new PoColor()` 返回 false（實例型別不符）。

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

#Params([[z, 來源], [Type, 物件的型別], [Name, 成員名], [O, 實例], [V, 要寫入的值]])

#Rtn[型別、成員、實例、可寫性任一不滿足返回 false]

#Descr[
實測（`PoUser`）：`Src.{nameof(TrySet)}(typeof(PoUser), "Age", User, 31)` 返回 true，之後 `User.Age` 是 31；
`"Secret"`（只讀）返回 false 且 `User.Secret` 仍是 "s"（不動實例）；
拿 `str` 當 `i32` 寫不返回 false，而是拋 {nameof(InvalidOperationException)}。

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

#Params([[z, 來源], [O, 目標物件]])

#Rtn[該物件的淺字典視圖]

#Descr[
型別取執行期型別，
修掉舊 Srefl 用 `typeof(T)` 查不到介面與基類成員的隱患。

`O` 為 null 拋 {nameof(ArgumentNullException)}。

實測：`Src.{nameof(ToInstDict)}(User)` 得到的 {nameof(IInstDict)}
其 {nameof(IInstDict.Target)} 就是那個 `User`、{nameof(IInstDict.Count)} 是 9
（鍵是運行期型別 `PoUser` 上可讀可寫的成員，`Secret` 只讀故不在鍵表內）；
`Dict["Id"]` 與 `User.Id` 是同一個值。
若變數的靜態型別是基類而運行期是子類，這裡查的是子類的元資料，故子類新增的成員也在視圖裏。
]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O);

	[Doc($"""
#Sum[建淺字典視圖，型別可由調用方顯式給。]

#Params([[z, 來源], [O, 目標物件], [Type, 物件的型別；為 null 時退到 `O.GetType()`]])

#Rtn[該物件的淺字典視圖]

#Descr[
實測：傳 `typeof(PoUserBase)` 而物件其實是 `PoUser`，
則視圖的 {nameof(IInstDict.Count)} 是 2、{nameof(IInstDict.Keys)} 依次是 `Id`、`Name`，
子類新增的成員不在鍵表裏；
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

#Params([[z, 來源], [O, 目標物件], [Dict, 要寫回的字典]])

#Descr[
按名逐鍵 {nameof(TrySet)}，只讀成員跳過；
鍵不在型別上 → {nameof(KeyNotFoundException)}（訊息同時列可寫名與可讀名，便於對照）；
值型別不符 → {nameof(InvalidOperationException)}。

實測：字典是 `Id=9L`、`Name="小紅"`、`Level=7` 時三個鍵都寫回物件；
字典是 `Level=4`、`Secret="改不掉"` 時 `User.Level` 變成 4，
而 `User.Secret` 仍是 "s"（只讀鍵被靜默跳過，不拋）；
字典裏多一個 `"NoSuch"` 就不是跳過了，拋 {nameof(KeyNotFoundException)}
（訊息含可用可寫鍵，實測含 "Age" 可被斷言）；
值型別不符（`Age` 傳 `"不是數字"`）拋 {nameof(InvalidOperationException)}。

要「只寫認得的鍵、其餘不管」的寬鬆語義，就先自己過濾字典再用；
本方法選擇嚴格，是因為寫回物件通常發生在反序列化路徑上，靜默丟鍵比拋異常更難查。
]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict);

	[Doc($"""
#Sum[把字典寫回物件，型別由調用方顯式給。]

#Params([[z, 來源], [O, 目標物件], [Dict, 要寫回的字典], [Type, 物件的型別；為 null 時退到 `O.GetType()`]])

#Descr[
實測：傳 `typeof(PoUserBase)` 時，字典裏的 `Id`、`Name` 能寫進去，
而 `Level`（子類宣告）因為「不在基類成員表上」拋 {nameof(KeyNotFoundException)}，
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