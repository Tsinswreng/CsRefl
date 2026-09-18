namespace Tsinswreng.CsRefl;

using System.Text.Json;
using System.Text.Json.Serialization;

/// JsonTypeInfoSrc 的函數實現。
/// 只放函數實現：字段與訪問器在 JsonTypeInfoSrc.cs。
/// 參數特性（DAM/NotNullWhen）只寫在聲明側，partial 合併時兩邊都標會報 CS0579。
public partial class JsonTypeInfoSrc{
	/// 用一個源生成 context 建來源（最常見：AppJsonCtx.Default）。
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx){
		ArgumentNullException.ThrowIfNull(Ctx);
		// JsonSerializerContext 顯式實現 IJsonTypeInfoResolver，轉介面調用。
		_resolver = Ctx;
		_options = Ctx.Options;
	}

	/// 用一份配好的 options 建來源（其 TypeInfoResolver 鏈裏含源生成 context 即可）。
	public partial JsonTypeInfoSrc(JsonSerializerOptions Options){
		ArgumentNullException.ThrowIfNull(Options);
		_resolver = Options.TypeInfoResolver
			?? throw new ArgumentException(
				"Options.TypeInfoResolver 為空；JsonTypeInfo 來源需要一個能解析型別的 resolver（如源生成 context）。",
				nameof(Options)
			);
		_options = Options;
	}

	/// 取已註冊型別的元資料；未註冊返回 false。
	/// 命中與未命中都進緩存：resolver 鏈的解析不是免費操作，
	/// 未註冊型別在批量場景裏會被反復查到。
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		if(_cache.TryGetValue(Type, out Info)){
			return true;
		}
		if(_misses.ContainsKey(Type)){
			Info = null;
			return false;
		}
		Info = Build(Type);
		if(Info is null){
			// 並行下重複標記無害：同一個型別的答案恆定。
			_misses.TryAdd(Type, 0);
			return false;
		}
		// 並行下重複 TryAdd 無害（包的是同一 JsonTypeInfo 實例）。
		_cache.TryAdd(Type, Info);
		return true;
	}

	/// 解析並包裝；resolver 返回 null（未註冊）時返回 null，由調用方記入負面緩存。
	private ITypeInfo? Build(Type Type){
		var JsonInfo = _resolver.GetTypeInfo(Type, _options);
		if(JsonInfo is null){
			return null;
		}
		return new JsonTypeInfoInfo(JsonInfo);
	}
}