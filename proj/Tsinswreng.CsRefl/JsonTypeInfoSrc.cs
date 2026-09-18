namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfo 來源：把 System.Text.Json 的源生成元數據（JsonSerializerContext /
/// JsonSerializerOptions.TypeInfoResolver）包成 ITypeInfoSrc。
///
/// 特性：
/// - 只認已註冊（[JsonSerializable] / resolver 鏈）的型別，未註冊 TryGetInfo 返回 false；
/// - 讀寫是委託（JsonPropertyInfo.Get/Set），AOT 主路徑最優解；
/// - RegisteredTypes 返回 null：context 不暴露已註冊清單，無法列舉。
///
/// 名字前提：JsonPropertyInfo.Name 是 JSON 名。要用它當 CsSql 的列名鍵，
/// 調用方必須保證命名策略為 null 且無 [JsonPropertyName]（Ngan.Dict 現狀即如此）。
/// 建構子與 TryGetInfo 實現見 JsonTypeInfoSrc.Impl.cs。
public partial class JsonTypeInfoSrc:ITypeInfoSrc{
	/// 解析器：context 或 options 的 resolver 鏈。
	private readonly IJsonTypeInfoResolver _resolver;
	/// 解析時用的 options。
	private readonly JsonSerializerOptions _options;
	/// 型別 → 元資料緩存；同一型別只包一次。
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();
	/// 未註冊型別的負面緩存。
	/// 解析失敗也要記下來，否則批量查未註冊型別時每次都要重走一遍 resolver 鏈。
	private readonly ConcurrentDictionary<Type, byte> _misses = new();

	/// 用一個源生成 context 建來源（最常見：AppJsonCtx.Default）。
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx);
	/// 用一份配好的 options 建來源（其 TypeInfoResolver 鏈裏含源生成 context 即可）。
	public partial JsonTypeInfoSrc(JsonSerializerOptions Options);

	/// 不支持列舉。
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return null;
		}
	}

	/// 取已註冊型別的元資料；未註冊返回 false。
	/// DAM 註解與接口一致：本來源不依賴成員元數據，但接口統一宣告了這個前置條件。
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
}