namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[JsonTypeInfo 來源：把 `System.Text.Json` 的源生成元數據包成 {nameof(ITypeInfoSrc)}。]

#Descr[
元數據的兩個入口：{nameof(JsonSerializerContext)} 或
{nameof(JsonSerializerOptions)}.{nameof(JsonSerializerOptions.TypeInfoResolver)}。

實測：`new {nameof(JsonTypeInfoSrc)}(AppJsonCtx.Default)` 用源生成上下文建來源；
若已經有一份配好的 {nameof(JsonSerializerOptions)}，
`new {nameof(JsonTypeInfoSrc)}(Options)` 更省事（只要它的 resolver 鏈裏有源生成上下文）。

實測：用這條來源查 `typeof(PoUser)` 返回 true 且
`Info.{nameof(ITypeInfo.Json)}` 非 null；
查未註冊的 `typeof(PoNoCtor)` 返回 false 且 `Info` 為 null。
]

#Descr[
特性：
+ 只認已註冊（`[JsonSerializable]` 或 resolver 鏈）的型別，
	未註冊時 {nameof(TryGetInfo)} 返回 false
+ 讀寫是委託（官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)}／
	{nameof(JsonPropertyInfo.Set)}），AOT 主路徑最優解
+ {nameof(RegisteredTypes)} 返回 null：上下文不暴露已註冊清單，無法列舉

實測：型別掛了 `[JsonSerializable]` 就能查到；
沒掛的走反射源才查得到，故生產上通常用 {nameof(MergedTypeInfoSrc)} 把兩者串起來。

實測：`typeof(PoUser)` 查得到（返回 true）；`typeof(PoNoCtor)` 查不到（返回 false）；
把本來源與 {nameof(ReflTypeInfoSrc)} 串成 {nameof(MergedTypeInfoSrc)} 後
`typeof(PoNoCtor)` 改由反射兜底、返回 true，
而 `typeof(PoUser)` 仍走本來源（回傳的實例與本來源單獨查到的 `{nameof(ReferenceEquals)}` 為 true）。
]

#Descr[
名字前提：
官方 {nameof(JsonPropertyInfo.Name)} 是 JSON 名。
要用它當 CsSql 的列名鍵，
調用方必須保證命名策略為 null 且無 `[JsonPropertyName]`
（Ngan.Dict 現狀即如此）。

注意：若開了駝峯命名策略，成員 `Age` 的 {nameof(IMemberInfo.Name)} 可能變成 "age"，
此時拿它當列名就會與 C# 屬性名對不上，故本包刻意不碰命名策略、把前提交給調用方。

實測（本庫測試用的來源）：成員 `Age` 的 {nameof(IMemberInfo.Name)} 就是 "Age"，
與反射來源一致，故兩套來源的按名查詢可以互換。

建構子與 {nameof(TryGetInfo)} 實現見 `JsonTypeInfoSrc.Impl.cs`。
]
""")]
public partial class JsonTypeInfoSrc:ITypeInfoSrc{
	[Doc($"""
#Sum[解析器：上下文或 options 的 resolver 鏈。]

#Descr[
實測：傳上下文進來時這裡就是那個上下文本身
（{nameof(JsonSerializerContext)} 顯式實現 {nameof(IJsonTypeInfoResolver)}）；
傳 options 進來時這裡取
{nameof(JsonSerializerOptions)}.{nameof(JsonSerializerOptions.TypeInfoResolver)}，可能是鏈式組合的解析器。

實測：用上下文建的來源，這個字段與傳進來的上下文 `{nameof(ReferenceEquals)}` 為 true；
傳 null 上下文或 null options 都在建構期拋 {nameof(ArgumentNullException)}。
]
""")]
	private readonly IJsonTypeInfoResolver _resolver;

	[Doc($"""
#Sum[解析時用的 options。]

#Descr[
解析要傳給 {nameof(IJsonTypeInfoResolver)}.{nameof(IJsonTypeInfoResolver.GetTypeInfo)}，
故必須與來源同源，不能自己另造一份 options，否則解析結果可能不一致。

實測：用上下文建的來源，這個字段就是上下文自帶的那份 options；
傳一份 {nameof(JsonSerializerOptions)} 時要求它的
{nameof(JsonSerializerOptions.TypeInfoResolver)} 非空，
否則建構期拋 {nameof(ArgumentException)}（不留到解析時才失敗）。
]
""")]
	private readonly JsonSerializerOptions _options;

	[Doc($"""
#Sum[型別 → 元資料緩存；同一型別只包一次。]

#Descr[
實測：同一型別查兩次拿到同一個 {nameof(ITypeInfo)}，
故官方的 {nameof(JsonTypeInfo)} 只被包一次，包裝層不重複建對象。

實測：`{nameof(TryGetInfo)}(typeof(PoUser), out var A)` 與再查一次的 `B`
滿足 `{nameof(ReferenceEquals)}(A, B)` 為 true。
]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	[Doc($"""
#Sum[未註冊型別的負面緩存。]

#Descr[
解析失敗也要記下來，
否則批量查未註冊型別時每次都要重走一遍 resolver 鏈。

實測：循環查一批型別、其中大多是沒掛 `[JsonSerializable]` 的時候，
第一次查不到就記住，之後直接返回 false，省掉整條 resolver 鏈的解析。

實測：第一次查 `typeof(PoNoCtor)` 返回 false 並記進這張表；
第二次仍返回 false 且 `Info` 為 null，但不再進 resolver。
]
""")]
	private readonly ConcurrentDictionary<Type, byte> _misses = new();

	[Doc($"""
#Sum[用一個源生成 context 建來源。]

#Params([[Ctx, 源生成 context，最常見是 AppJsonCtx.Default]])

#Descr[
實測：`new {nameof(JsonTypeInfoSrc)}(AppJsonCtx.Default)` 之後
即可查 `AppJsonCtx` 上宣告過 `[JsonSerializable]` 的所有型別。

實測：這樣建出來的來源查 `typeof(PoUser)` 返回 true、
查 `typeof(PoNoCtor)` 返回 false（後者沒進上下文）。
]
""")]
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx);

	[Doc($"""
#Sum[用一份配好的 options 建來源。]

#Params([[Options, 選項；其 {nameof(JsonSerializerOptions.TypeInfoResolver)} 鏈裏含源生成 context 即可]])

#Descr[
實測：應用已經有一份全域 {nameof(JsonSerializerOptions)}，
直接拿它建來源，元資料口徑就與序列化一致，
不會出現「序列化用一套命名策略、元資料用另一套」的分歧。

實測：那份 options 的 {nameof(JsonSerializerOptions.TypeInfoResolver)} 為 null 時
建構期即拋 {nameof(ArgumentException)}（訊息含 "TypeInfoResolver"），
避免把問題拖到查詢時才暴露。
]
""")]
	public partial JsonTypeInfoSrc(JsonSerializerOptions Options);

	[Doc($"""
#Sum[不支持列舉（上下文不暴露已註冊清單）。]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return null;
		}
	}

	[Doc($"""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#Descr[
實測：查掛過 `[JsonSerializable]` 的型別返回 true；
查沒掛過的返回 false 且 {nameof(Info)} 為 null。

想讓這種型別也查得到有兩條路：
把它補進源生成上下文，或在來源鏈後接 {nameof(ReflTypeInfoSrc)} 兜底。

實測：`typeof(PoUser)` 返回 true 且 `Info` 非 null；
`typeof(PoNoCtor)` 返回 false 且 `Info` 為 null
（測試裏那條「未註冊型別查不到」的用例就是釘這一條）。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);

	// ---- 私有輔助（實現見 JsonTypeInfoSrc.Impl.cs）----

	[Doc($"""
#Sum[解析並包裝。]

#Params([[Type, 要解析的型別]])

#Rtn[包好的元資料；resolver 返回 null（未註冊）時為 null]

#Descr[
返回 null 由調用方記入負面緩存。

實測：查未註冊的 `typeof(PoNoCtor)` 時 resolver 鏈返回 null，
本方法就跟著返回 null（不自己造一份假的官方元資料）；
查 `typeof(PoUser)` 時返回一個 {nameof(JsonTypeInfoInfo)}，
其 {nameof(ITypeInfo.Json)} 非 null。
]
""")]
	private partial ITypeInfo? Build(Type Type);
}