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

例：`new {nameof(JsonTypeInfoSrc)}(AppJsonCtx.Default)` 用源生成上下文建來源；
若已經有一份配好的 {nameof(JsonSerializerOptions)}，
`new {nameof(JsonTypeInfoSrc)}(Options)` 更省事（只要它的 resolver 鏈裏有源生成上下文）。
]

#Descr[
特性：
+ 只認已註冊（`[JsonSerializable]` 或 resolver 鏈）的型別，
	未註冊時 {nameof(TryGetInfo)} 返回 false
+ 讀寫是委託（官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)}／
	{nameof(JsonPropertyInfo.Set)}），AOT 主路徑最優解
+ {nameof(RegisteredTypes)} 返回 null：上下文不暴露已註冊清單，無法列舉

例：型別掛了 `[JsonSerializable]` 就能查到；
沒掛的走反射源才查得到，故生產上通常用 {nameof(MergedTypeInfoSrc)} 把兩者串起來。
]

#Descr[
名字前提：
官方 {nameof(JsonPropertyInfo.Name)} 是 JSON 名。
要用它當 CsSql 的列名鍵，
調用方必須保證命名策略為 null 且無 `[JsonPropertyName]`
（Ngan.Dict 現狀即如此）。

例：若開了駝峯命名策略，成員 `Age` 的 {nameof(IMemberInfo.Name)} 可能變成 "age"，
此時拿它當列名就會與 C# 屬性名對不上，故本包刻意不碰命名策略、把前提交給調用方。

建構子與 {nameof(TryGetInfo)} 實現見 `JsonTypeInfoSrc.Impl.cs`。
]
""")]
public partial class JsonTypeInfoSrc:ITypeInfoSrc{
	[Doc($"""
#Sum[解析器：上下文或 options 的 resolver 鏈。]

#Descr[
例：傳上下文進來時這裡就是那個上下文本身
（{nameof(JsonSerializerContext)} 顯式實現 {nameof(IJsonTypeInfoResolver)}）；
傳 options 進來時這裡取
{nameof(JsonSerializerOptions)}.{nameof(JsonSerializerOptions.TypeInfoResolver)}，可能是鏈式組合的解析器。
]
""")]
	private readonly IJsonTypeInfoResolver _resolver;

	[Doc($"""
#Sum[解析時用的 options。]

#Descr[
例：解析要傳給 {nameof(IJsonTypeInfoResolver)}.{nameof(IJsonTypeInfoResolver.GetTypeInfo)}，
故必須與來源同源，不能自己另造一份 options，否則解析結果可能不一致。
]
""")]
	private readonly JsonSerializerOptions _options;

	[Doc($"""
#Sum[型別 → 元資料緩存；同一型別只包一次。]

#Descr[
例：同一型別查兩次拿到同一個 {nameof(ITypeInfo)}，
故官方的 {nameof(JsonTypeInfo)} 只被包一次，包裝層不重複建對象。
]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	[Doc($"""
#Sum[未註冊型別的負面緩存。]

#Descr[
解析失敗也要記下來，
否則批量查未註冊型別時每次都要重走一遍 resolver 鏈。

例：循環查一批型別、其中大多是沒掛 `[JsonSerializable]` 的時候，
第一次查不到就記住，之後直接返回 false，省掉整條 resolver 鏈的解析。
]
""")]
	private readonly ConcurrentDictionary<Type, byte> _misses = new();

	[Doc($"""
#Sum[用一個源生成 context 建來源。]

#Params([[源生成 context，最常見是 AppJsonCtx.Default]])

#Descr[
例：`new {nameof(JsonTypeInfoSrc)}(AppJsonCtx.Default)` 之後
即可查 `AppJsonCtx` 上宣告過 `[JsonSerializable]` 的所有型別。
]
""")]
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx);

	[Doc($"""
#Sum[用一份配好的 options 建來源。]

#Params([[選項；其 {nameof(JsonSerializerOptions.TypeInfoResolver)} 鏈裏含源生成 context 即可]])

#Descr[
例：應用已經有一份全域 {nameof(JsonSerializerOptions)}，
直接拿它建來源，元資料口徑就與序列化一致，
不會出現「序列化用一套命名策略、元資料用另一套」的分歧。
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
例：查掛過 `[JsonSerializable]` 的型別返回 true；
查沒掛過的返回 false 且 {nameof(Info)} 為 null。

想讓這種型別也查得到有兩條路：
把它補進源生成上下文，或在來源鏈後接 {nameof(ReflTypeInfoSrc)} 兜底。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
}