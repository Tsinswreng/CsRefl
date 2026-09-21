namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料的來源。可插拔、可合成。]

#Descr[
現成實現各自覆蓋一種場景：
+ {nameof(ReflTypeInfoSrc)}：兼容 AOT 的反射來源，任何型別都能查（元數據保留時）
+ {nameof(JsonTypeInfoSrc)}：`System.Text.Json` 源生成上下文，只認已註冊型別
+ {nameof(TypeInfoReg)}：手寫註冊表（把手工元資料塞進去）
+ {nameof(MergedTypeInfoSrc)}：多來源按優先級合成

實測：AOT 主路徑上掛 {nameof(MergedTypeInfoSrc)}，
把 {nameof(JsonTypeInfoSrc)} 放第一位（讀寫走源生成的委託，零反射）、
{nameof(ReflTypeInfoSrc)} 放第二位兜底，
於是 `typeof(PoUser)` 由 Json 源接住（回傳實例與 Json 源單獨查到的 `{nameof(ReferenceEquals)}` 為 true），
而沒掛 `[JsonSerializable]` 的 `typeof(PoNoCtor)` 落到反射源、照樣查得到。
]

#Descr[
「來源」是行為不是資料：本接口以查詢為主，唯一可寫的是 {nameof(RegisteredTypes)}
（留給調用方自己換表的自由度）；按動詞登記請用 {nameof(ITypeInfoReg)}，
多來源合成請用 {nameof(MergedTypeInfoSrc)}。
]
""")]
public interface ITypeInfoSrc{
	[Doc($$"""
#Sum[取指定型別的元資料；未知返回 false。]

#Params([[Type, 要查的型別], [Info, 取到的元資料；未知時為 null]])

#Descr[
調用方這樣寫：

```csharp
var JsonOnly = new JsonTypeInfoSrc(TestJsonCtx.Default);
if(JsonOnly.TryGetInfo(typeof(PoUser), out var Info)){
	Info.Type;   // typeof(PoUser)
}

JsonOnly.TryGetInfo(typeof(PoNoCtor), out _);
// false：PoNoCtor 沒掛 [JsonSerializable]，源生成來源答「未知」。
// 同一個型別再查一次也直接 false（未註冊會被記進負面緩存，不反復走 resolver 鏈）。

new ReflTypeInfoSrc().TryGetInfo(typeof(PoNoCtor), out _);
// true：反射來源照樣查得到，這就是兜底的意義。
```

DAM 註解：反射來源需要被查型別保留
接口、公共屬性、公共字段、無參構造函數 的元數據；
{{nameof(JsonTypeInfoSrc)}} 不依賴它，但接口統一宣告了這個前置條件。

要「查不到就拋」的便利版本用 {{nameof(ITypeInfoSrcExtn.GetInfo)}}；
要按名讀寫就用 {{nameof(ITypeInfoSrcExtn.TryGet)}}／{{nameof(ITypeInfoSrcExtn.AssignFromDict)}}。
]
""")]
	bool TryGetInfo(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);

	[Doc($$"""
#Sum[本來源所知的「型別 → 元資料」表；不支持列舉的來源返回 null。]

#Descr[
鍵是型別、值是該型別的元資料（與 {{nameof(TryGetInfo)}} 交出的是同一批物件）。
要型別清單就取這張表的鍵集合，不必另立一個只交鍵的成員：
鍵與值是同一次查詢的兩半，分兩個成員交付，會出現「列舉到的型別」與「隨後查到的元資料」
不是同一時刻的情況。

{{nameof(TypeInfoReg)}} 登記什麼就交出什麼；{{nameof(ReflTypeInfoSrc)}} 與
{{nameof(JsonTypeInfoSrc)}} 恆為 null——前者對任意型別都能現場建元資料，給不出
「所有能查的型別」，後者的官方上下文不暴露已註冊清單。

{{nameof(MergedTypeInfoSrc)}} 只在全部成員來源都能列舉時才交出並集，否則整個返回 null。

賦值＝把這條來源的表整張換掉：不合併、不拷貝，給什麼就存什麼
（傳 null 表示「不支持列舉」）。本屬性只負責交事實、不攔調用方——
拿到的那份字典歸誰、之後要不要改、改了算不算數，由調用方自己負責；
各來源採納賦值的範圍寫在它們自己的說明裏。

調用方不得假設「能列舉」，只把它當加分能力：
返回 null 就說明這條鏈不能當「型別全集」用，例如不能用它來做全庫掃描。

調用方這樣寫：

```csharp
var Reg = new {{nameof(TypeInfoReg)}}();
Reg.{{nameof(TypeInfoReg.Add)}}(typeof({{nameof(ITypeInfo)}}), Info);

var Map = Reg.{{nameof(RegisteredTypes)}}!;
Map[typeof({{nameof(ITypeInfo)}})];                                                       // 剛才登記進去的那個 Info
Map.{{nameof(IDictionary<Type, ITypeInfo>.ContainsKey)}}(typeof({{nameof(ITypeInfoSrc)}}));   // false：沒登記過
```
]
""")]
	//TswgNote 爲甚麼用List? 怎麼到處都在用list?
	// 已按此改：RegisteredTypes 由 IReadOnlyCollection<Type> 改為 IDictionary<Type, ITypeInfo>。
	// 理由：鍵與值是同一張表的兩半，只交鍵會讓「列舉到的型別」與「隨後查到的元資料」不是同一時刻。
	IDictionary<Type, ITypeInfo>? RegisteredTypes{get;set;}
}
