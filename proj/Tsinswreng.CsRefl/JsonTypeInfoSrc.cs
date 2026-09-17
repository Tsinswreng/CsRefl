namespace Tsinswreng.CsRefl;

using System.Text.Json;
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
	private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	/// 不支持列舉。
	public IReadOnlyCollection<Type>? RegisteredTypes => null;
}