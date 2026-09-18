namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[JsonTypeInfo 來源：把 `System.Text.Json` 的源生成元數據包成 `ITypeInfoSrc`。]

#Descr[
元數據的兩個入口：`JsonSerializerContext` 或 `JsonSerializerOptions.TypeInfoResolver`。
]

#Descr[
特性：
+ 只認已註冊（`[JsonSerializable]` / resolver 鏈）的型別，未註冊 `TryGetInfo` 返回 false
+ 讀寫是委託（`JsonPropertyInfo.Get`/`Set`），AOT 主路徑最優解
+ `RegisteredTypes` 返回 null：context 不暴露已註冊清單，無法列舉
]

#Descr[
名字前提：
`JsonPropertyInfo.Name` 是 JSON 名。
要用它當 CsSql 的列名鍵，
調用方必須保證命名策略為 null 且無 `[JsonPropertyName]`
（Ngan.Dict 現狀即如此）。

建構子與 `TryGetInfo` 實現見 `JsonTypeInfoSrc.Impl.cs`。
]
""")]
public partial class JsonTypeInfoSrc:ITypeInfoSrc{
	[Doc("""
#Sum[解析器：context 或 options 的 resolver 鏈。]
""")]
	private readonly IJsonTypeInfoResolver _resolver;

	[Doc("""
#Sum[解析時用的 options。]
""")]
	private readonly JsonSerializerOptions _options;

	[Doc("""
#Sum[型別 → 元資料緩存；同一型別只包一次。]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	[Doc("""
#Sum[未註冊型別的負面緩存。]

#Descr[
解析失敗也要記下來，
否則批量查未註冊型別時每次都要重走一遍 resolver 鏈。
]
""")]
	private readonly ConcurrentDictionary<Type, byte> _misses = new();

	[Doc("""
#Sum[用一個源生成 context 建來源。]

#Params([[源生成 context，最常見是 `AppJsonCtx.Default`]])
""")]
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx);

	[Doc("""
#Sum[用一份配好的 options 建來源。]

#Params([[選項；其 `TypeInfoResolver` 鏈裏含源生成 context 即可]])
""")]
	public partial JsonTypeInfoSrc(JsonSerializerOptions Options);

	[Doc("""
#Sum[不支持列舉（context 不暴露已註冊清單）。]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return null;
		}
	}

	[Doc("""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
}