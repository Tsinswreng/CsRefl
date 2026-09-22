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
	[Doc($$"""
#Sum[查型別的元資料；查不到就拋。]

#Params([[z, 來源], [Type, 要查的型別]])

#Rtn[取到的型別元資料]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo(typeof(PoUser));
// Info.Type 是 typeof(PoUser)；接著就能 Info.Members、Info.WritableNames、Info.TryGetMember("Age", out var M)。

var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
JsonOnly.GetInfo(typeof(PoNoCtor));
// 拋 KeyNotFoundException：PoNoCtor 沒掛 [JsonSerializable]，訊息裏指名型別與來源。
```

與 {{nameof(ITypeInfoSrc.TryGetInfo)}} 成對（正如 {{nameof(GetMember)}} 之於 {{nameof(TryGetMember)}}）：
確定型別一定查得到時用本方法，省掉調用方的 `if(!TryGetInfo(...))` 樣板；
不確定就用 {{nameof(ITypeInfoSrc.TryGetInfo)}}。
]
""")]
	public static partial ITypeInfo GetInfo(
		this ITypeInfoSrc z,
		[DAM(ReflTypeInfo.ReflDam)] Type Type
	);

	[Doc($$"""
#Sum[查型別元資料（泛型版：`T` 就是型別）。]

#TParams[要查的型別]

#Rtn[取到的型別元資料]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();
Info.Type;   // typeof(PoUser)
```

`T` 在編譯期就是已知型別，故剪裁器（DAM）看得見「這個型別需要成員元數據」，
不必像 `GetInfo(Type)` 那樣要調用方自己記得傳 `typeof(T)`。
]
""")]
	public static partial ITypeInfo GetInfo<[DAM(ReflTypeInfo.ReflDam)] T>(this ITypeInfoSrc z);

	[Doc($"""
#Sum[運行期型別的 DAM 擔保。]

#Params([[T, 運行期型別，通常來自 `obj.GetType()`]])

#Rtn[原樣返回入參]

#Descr[
動態 `GetType()` 在分析器眼裏不攜帶 DAM 信息，
但本包在此路徑上對 `T` 的用法只有 {nameof(Type.IsInstanceOfType)} 與反射建元資料，
缺元數據時會在反射建元資料處自然拋錯，不會悄悄剪錯，
故此處顯式擔保成員元數據需求。

實測：`{nameof(ToInstDict)}(User)` 內部就是先 `{nameof(_RuntimeType)}(User.GetType())`，
再拿這個 {nameof(Type)} 去查來源（實測得到的是 `typeof(PoUser)`）；
調用方不必自己處理 DAM，剪裁分析器也不會因此報警。
]
""")]
	[UnconditionalSuppressMessage("Trimming", "IL2068",
		Justification = "運行期型別（O.GetType()）本質無法靜態攜帶 DAM 信息；本包對它的用法只有 IsInstanceOfType 與反射建元資料，缺元數據時在反射建元資料處自然拋錯，不會悄悄剪錯。")]
	[return: DAM(ReflTypeInfo.ReflDam)]
	private static partial Type _RuntimeType(Type T);

	[Doc($$"""
#Sum[取成員；型別未註冊或成員不存在都拋。]

#Params([[z, 來源], [Type, 要查的型別], [Name, 成員名]])

#Rtn[取到的成員元資料]

#Descr[
調用方這樣寫：

```csharp
var M = Src.GetMember(typeof(PoUser), nameof(PoUser.Age));
// M 是官方成員物件；M.Name 是 "Age"、M.OwnerType 是 typeof(PoUser)。

Src.GetMember(typeof(PoUser), "NoSuch");
// 拋 KeyNotFoundException，訊息含該型別的可用成員名（可直接拿去排查拼錯的名字）。

var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
JsonOnly.GetMember(typeof(PoNoCtor), nameof(PoNoCtor.X));
// 也拋 KeyNotFoundException，但訊息明說「型別未註冊」——兩種錯分得開。
```

`Type` 的 DAM 註解：後端會把型別交給來源查元資料（反射來源需要成員元數據）。
]
""")]
	public static partial IMemberInfo GetMember(
		this ITypeInfoSrc z,
		[DAM(ReflTypeInfo.ReflDam)] Type Type,
		str Name
	);

	[Doc($$"""
#Sum[取成員（泛型版）。]

#TParams[所屬型別]

#Params([[z, 來源], [Name, 成員名]])

#Rtn[命中的官方成員物件]

#Descr[
調用方這樣寫：

```csharp
var M = Src.GetMember<PoUser>(nameof(PoUser.Age));
M.PropertyType;   // typeof(i32)
```

等於 `Src.GetMember(typeof(PoUser), nameof(PoUser.Age))`。
]
""")]
	public static partial IMemberInfo GetMember<[DAM(ReflTypeInfo.ReflDam)] T>(this ITypeInfoSrc z, str Name);

	[Doc($$"""
#Sum[取成員的 Try 版：型別未註冊或成員不存在都返回 false。]

#Params([[z, 來源], [Type, 要查的型別], [Name, 成員名], [M, 取到的成員；失敗時為 null]])

#Rtn[型別未註冊或成員不存在都返回 false]

#Descr[
調用方這樣寫（名字是外部來的時候用這個，不拋）：

```csharp
if(Src.TryGetMember(typeof(PoUser), Name, out var M)){
	// 命中：M 是官方成員物件，直接交給 Member 問名字/型別/可讀可寫。
}

Src.TryGetMember(typeof(PoUser), "NoSuch", out _);
// false：名字不在成員表上。
Src.TryGetMember(typeof(PoNoCtor), nameof(PoNoCtor.X), out _);
// false：型別沒註冊到這個來源。
Src.TryGetMember(null, "Age", out _);
// false：來源傳 null 也不拋，故可以放心接外部傳來的型別與名字。
```

確定必須成功時用 {{nameof(GetMember)}}。
]
""")]
	public static partial bool TryGetMember(
		this ITypeInfoSrc z,
		[DAM(ReflTypeInfo.ReflDam)] Type Type,
		str Name,
		[NotNullWhen(true)] out IMemberInfo? M
	);

	[Doc($$"""
#Sum[取成員的 Try 版（泛型版）。]

#TParams[所屬型別]

#Params([[z, 來源], [Name, 成員名], [M, 取到的成員；失敗時為 null]])

#Rtn[型別未註冊或成員不存在都返回 false]

#Descr[
調用方這樣寫：

```csharp
if(Src.TryGetMember<PoUser>(nameof(PoUser.Age), out var M)){
	M.CanWrite;   // true
}
Src.TryGetMember<PoUser>("NoSuch", out _);   // false
```
]
""")]
	public static partial bool TryGetMember<[DAM(ReflTypeInfo.ReflDam)] T>(this ITypeInfoSrc z, str Name, out IMemberInfo? M);

	[Doc($$"""
#Sum[按名讀值；失敗返回 false（不拋）。]

#Params([[z, 來源], [Type, 物件的型別], [O, 實例], [Name, 成員名], [R, 讀出的值]])

#Rtn[型別、成員、實例、可讀性任一不滿足返回 false]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Age = 26 };

Src.TryGet(typeof(PoUser), User, nameof(PoUser.Age), out var V);
// true；V 是 boxed 的 i32 26。

Src.TryGet(typeof(PoUser), User, nameof(PoUser.Email), out var Email);
// true；Email 未賦值，故 V 是 null。

Src.TryGet(typeof(PoUser), User, "NoSuch", out _);          // false：名字不在成員表上
Src.TryGet(typeof(PoUser), null, nameof(PoUser.Age), out _); // false：實例是 null
Src.TryGet(typeof(PoColor), User, nameof(PoUser.Age), out _); // false：實例與型別不符
```

參數序按範圍由大到小：型別 → 實例 → 成員名 → 收值的那個出參，
與同類的 {{nameof(AssignFromDict)}}／{{nameof(ToInstDict)}}（實例在前）一致。

三步（查型別、查成員、讀值）合一、失敗一律用 false 表示，
故適合批量回填、按外部欄位名取值這類「能取就取」的場景；
要「取不到就拋」的語義就用 {{nameof(GetMember)}} 拿成員再自己讀。
]
""")]
	//TswgNote 爲甚麼Name在O前面? 不覺得很反直覺嗎? 範圍不應該都是從大到小嗎?
	// 已按此改：參數序改為 Type → O → Name（範圍大→小），與 AssignFromDict／ToInstDict 的「實例在前」一致。
	public static partial bool TryGet(
		this ITypeInfoSrc z,
		[DAM(ReflTypeInfo.ReflDam)] Type Type,
		obj? O,
		str Name,
		out obj? R
	);

	[Doc($$"""
#Sum[按名讀值（泛型版）。]

#TParams[物件的靜態型別]

#Params([[z, 來源], [O, 實例], [Name, 成員名], [V, 讀出的值]])

#Rtn[型別、成員、實例、可讀性任一不滿足返回 false]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Age = 26 };

Src.TryGet<PoUser>(User, nameof(PoUser.Age), out var V);
// true；V 是 boxed 的 i32 26。

Src.TryGet<PoUser>(User, nameof(PoUser.Token), out _);   // false：只寫成員讀不到
```

參數序同非泛型版：實例 → 成員名 → 收值的那個出參。
]
""")]
	public static partial bool TryGet<T>(this ITypeInfoSrc z, T O, str Name, out obj? V);

	[Doc($$"""
#Sum[按名寫值；不可寫返回 false，值型別不符照常拋。]

#Params([[z, 來源], [Type, 物件的型別], [O, 實例], [Name, 成員名], [V, 要寫入的值]])

#Rtn[型別、成員、實例、可寫性任一不滿足返回 false]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Married = false };

Src.TrySet(typeof(PoUser), User, nameof(PoUser.Married), true);
// true；之後 User.Married 是 true。

Src.TrySet(typeof(PoUser), User, "NoSuch", 1);
// false：名字不在成員表上。
Src.TrySet(typeof(PoUser), User, nameof(PoUser.Secret), "x");
// false：Secret 是只讀成員，且 User.Secret 仍是 "s"（不動實例）。

Src.TrySet(typeof(PoUser), User, nameof(PoUser.Age), "不是數字");
// 拋（不是返回 false）：值型別不符是調用方的 bug，必須爆出來，不吞成「不可寫」。
```

參數序同 {{nameof(TryGet)}}：型別 → 實例 → 成員名 → 值。

區分「不可寫」與「寫錯型別」是刻意的：
前者是型別設計決定的、可以無視；後者必須讓它當場炸。
]
""")]
	public static partial bool TrySet(
		this ITypeInfoSrc z,
		[DAM(ReflTypeInfo.ReflDam)] Type Type,
		obj? O,
		str Name,
		obj? V
	);

	[Doc($$"""
#Sum[按名寫值（泛型版）。]

#TParams[物件的靜態型別]

#Params([[z, 來源], [O, 實例], [Name, 成員名], [V, 要寫入的值]])

#Rtn[型別、成員、實例、可寫性任一不滿足返回 false]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Age = 26 };

Src.TrySet<PoUser>(User, nameof(PoUser.Age), 31);
// true；之後 User.Age 是 31。

Src.TrySet<PoUser>(User, nameof(PoUser.Secret), "x");   // false：只讀成員
```

值型別不符照常拋（與非泛型版同一條規矩）。
]
""")]
	public static partial bool TrySet<T>(this ITypeInfoSrc z, T O, str Name, obj? V);

	[Doc($$"""
#Sum[建淺字典視圖，型別取 `O.GetType()`。]

#Params([[z, 來源], [O, 目標物件]])

#Rtn[該物件的淺字典視圖]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Id = 1, Name = "小明", Age = 26 };

var Dict = Src.ToInstDict(User);
// Dict.Target 就是 User；Dict.Count 是 9（可讀可寫的成員，只讀的 Secret 不在鍵表內）。

Dict[nameof(PoUser.Age)] = 32;
// 寫回原物件：User.Age 變成 32。

Src.ToInstDict(null!);
// 拋 ArgumentNullException：視圖必須能讀寫實例，所以不接 null。
```

型別取執行期型別：變數的靜態型別是基類而實例是子類時，查的是子類的元資料，
故子類新增的成員也在鍵表裏（這也修掉了舊 Srefl 用 `typeof(T)` 查不到介面與基類成員的隱患）。
只要基類那部分請用另一個重載（見下）。
]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O);

	[Doc($$"""
#Sum[建淺字典視圖，型別可由調用方顯式給（只要基類那部分時用）。]

#Params([[z, 來源], [O, 目標物件], [Type, 物件的型別；為 null 時退到 `O.GetType()`]])

#Rtn[該物件的淺字典視圖]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser{ Id = 1, Name = "小明", Age = 26 };

var BaseDict = Src.ToInstDict(User, typeof(PoUserBase));
// BaseDict.Count 是 2；Keys 依次是 Id、Name——子類新增的成員（Age 等）不在鍵表裏。

foreach(var K in BaseDict.Keys){
	// 只會走到 Id 與 Name。
}
```

把「物件其實是 PoUser、但這次只當 PoUserBase 看」講清楚時用這個重載；
不傳型別就用實例的執行期型別（見上一個重載）。
]
""")]
	public static partial IInstDict ToInstDict(
		this ITypeInfoSrc z,
		obj? O,
		[DAM(ReflTypeInfo.ReflDam)] Type? Type
	);

	[Doc($$"""
#Sum[建淺字典視圖（泛型版）。]

#TParams[物件的靜態型別]

#Params([[z, 來源], [O, 目標物件]])

#Rtn[該物件的淺字典視圖]

#Descr[
`T` 是**靜態型別**，所以鍵表以 `typeof(T)` 為準——這與非泛型版（取執行期型別）不同：

```csharp
var User = new PoUser{ Id = 1, Name = "小明", Age = 26 };

var D1 = Src.ToInstDict<PoUser>(User);        // 鍵 9 個（PoUser 的可讀可寫成員）
PoUserBase B = User;
var D2 = Src.ToInstDict<PoUserBase>(B);       // 鍵只有 Id、Name 兩個——按靜態型別走
var D3 = Src.ToInstDict(B);                   // 非泛型版按執行期型別：仍是 9 個鍵
```

要「按執行期型別」就別用泛型版，用 `{{nameof(ToInstDict)}}(O)`。
]
""")]
	public static partial IInstDict ToInstDict<T>(this ITypeInfoSrc z, T O);

	[Doc($$"""
#Sum[把鍵值對寫回物件（型別取 `O.GetType()`）。]

#Params([[z, 來源], [O, 目標物件], [Dict, 要寫回的鍵值對序列；字典即可]])
#Rtn[本次的結果；現在是空殼 {{nameof(ResAssignFromDict)}}，日後再填]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser();

Src.AssignFromDict(User, new Dictionary<str, obj?>{
	[nameof(PoUser.Id)] = 9L,
	[nameof(PoUser.Name)] = "小紅",
	[nameof(PoUser.Level)] = 7,
});
// User.Id 是 9、User.Name 是 "小紅"、User.Level 是 7。

Src.AssignFromDict(User, new Dictionary<str, obj?>{
	[nameof(PoUser.Level)] = 4,
	[nameof(PoUser.Secret)] = "改不掉",
});
// User.Level 變成 4；Secret 是只讀成員，靜默跳過——User.Secret 仍是 "s"（不拋）。

Src.AssignFromDict(User, new Dictionary<str, obj?>{ ["NoSuch"] = 1 });
// 拋 KeyNotFoundException：型別上沒有的鍵不是「跳過」，訊息含可用可寫鍵（實測含 "Age"）。

Src.AssignFromDict(User, new Dictionary<str, obj?>{ [nameof(PoUser.Age)] = "不是數字" });
// 拋 InvalidOperationException：值型別不符。
```

入參只要 {{nameof(IEnumerable<KeyValuePair<str, obj?>>)}}：本方法把鍵值對遍歷一遍，
故 {{nameof(Dictionary<,>)}}、{{nameof(IDictionary<,>)}}、{{nameof(IReadOnlyDictionary<,>)}}、字典視圖都能直接傳。

要「只寫認得的鍵、其餘不管」的寬鬆語義，就先自己過濾字典再用；
本方法選擇嚴格，是因為寫回物件通常發生在反序列化路徑上，靜默丟鍵比拋異常更難查。
]
""")]
	//你媽的怎麼這麼喜歡IReadonly? 你返回值readonly就算了入參你還readonly? 你怎麼這麼喜歡加戲呢?
	// 已按此改：入參放寬成 IEnumerable<KeyValuePair<str, obj?>>——本方法只把鍵值對遍歷一遍，
	// 不該要求調用方先湊成 IReadOnlyDictionary（IDictionary／IInstDict 那側反而傳不進來）。
	public static partial ResAssignFromDict AssignFromDict(
		this ITypeInfoSrc z, obj? O, IEnumerable<KeyValuePair<str, obj?>> Dict
	);

	[Doc($$"""
#Sum[把鍵值對寫回物件，型別由調用方顯式給（只要基類那部分時用）。]

#Params([[z, 來源], [O, 目標物件], [Dict, 要寫回的鍵值對序列], [Type, 物件的型別；為 null 時退到 `O.GetType()`]])
#Rtn[本次的結果；現在是空殼 {{nameof(ResAssignFromDict)}}，日後再填]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser();

Src.AssignFromDict(User, new Dictionary<str, obj?>{
	[nameof(PoUserBase.Id)] = 9L,
	[nameof(PoUserBase.Name)] = "小紅",
}, typeof(PoUserBase));
// User.Id 是 9、User.Name 是 "小紅"：只按基類那張成員表寫。

Src.AssignFromDict(User, new Dictionary<str, obj?>{ [nameof(PoUser.Level)] = 7 }, typeof(PoUserBase));
// 拋 KeyNotFoundException：Level 宣告在子類 PoUser 上，不在 PoUserBase 的成員表裏。
```

故這種用法通常要先把字典裁剪到基類欄位；
不傳型別就用實例的執行期型別（見上一個重載）。
]
""")]
	public static partial ResAssignFromDict AssignFromDict(
		this ITypeInfoSrc z,
		obj? O,
		IEnumerable<KeyValuePair<str, obj?>> Dict,
		[DAM(ReflTypeInfo.ReflDam)] Type? Type
	);

	[Doc($$"""
#Sum[把鍵值對寫回物件（泛型版）。]

#TParams[物件的靜態型別]

#Params([[z, 來源], [O, 目標物件], [Dict, 要寫回的鍵值對序列]])
#Rtn[本次的結果；現在是空殼 {{nameof(ResAssignFromDict)}}，日後再填]

#Descr[
調用方這樣寫：

```csharp
var User = new PoUser();

Src.AssignFromDict<PoUser>(User, new Dictionary<str, obj?>{
	[nameof(PoUser.Id)] = 9L,
	[nameof(PoUser.Name)] = "小紅",
});
// User.Id 是 9、User.Name 是 "小紅"。

Src.AssignFromDict<PoUser>(User, new Dictionary<str, obj?>{ ["NoSuch"] = 1 });
// 拋 KeyNotFoundException（判據同非泛型版）。
```

`T` 是靜態型別，故只認 `typeof(T)` 那張成員表。
]
""")]
	public static partial ResAssignFromDict AssignFromDict<T>(this ITypeInfoSrc z, T O, IEnumerable<KeyValuePair<str, obj?>> Dict);

}















