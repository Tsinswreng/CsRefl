namespace Tsinswreng.CsRefl;

using System.Text.Json;
using System.Text.Json.Serialization;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(JsonTypeInfoSrc)} 的函數實現。]

#Descr[
只放函數實現：字段與訪問器在 `JsonTypeInfoSrc.cs`。
]
""")]
public partial class JsonTypeInfoSrc{
	[Doc($"""
#Sum[用一個源生成 context 建來源。]

#See[{nameof(JsonTypeInfoSrc)}]
""")]
	public partial JsonTypeInfoSrc(JsonSerializerContext Ctx){
		ArgumentNullException.ThrowIfNull(Ctx);
		// JsonSerializerContext 顯式實現 IJsonTypeInfoResolver，轉介面調用。
		_resolver = Ctx;
		_options = Ctx.Options;
	}

	[Doc($"""
#Sum[用一份配好的 options 建來源。]

#See[{nameof(JsonTypeInfoSrc)}]
""")]
	public partial JsonTypeInfoSrc(JsonSerializerOptions Options){
		ArgumentNullException.ThrowIfNull(Options);
		// step 1: 取出 resolver 鏈；取不到就沒有解析能力，直接擋在建構期。
		_resolver = Options.TypeInfoResolver
			?? throw new ArgumentException(
				"Options.TypeInfoResolver 為空；JsonTypeInfo 來源需要一個能解析型別的 resolver（如源生成 context）。",
				nameof(Options)
			);
		// step 2: 連 options 一起留著，解析時要傳回給 resolver（見 Build）。
		_options = Options;
	}

	[Doc($"""
#Sum[取已註冊型別的元資料；未註冊返回 false。]

#Descr[
三級短路：先查命中緩存，再查負面緩存，最後才真正解析。

例：查一個沒掛 `[JsonSerializable]` 的型別首次返回 false 並記進負面緩存，
第二次直接返回 false，不再走 resolver 鏈。
]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		// step 1: 命中緩存直接返回。
		if(_cache.TryGetValue(Type, out Info)){
			return true;
		}
		// step 2: 未命中且已知是未註冊型別，直接返回 false，不再走 resolver 鏈。
		// 命中與未命中都進緩存：resolver 鏈的解析不是免費操作，
		// 未註冊型別在批量場景裏會被反復查到。
		if(_misses.ContainsKey(Type)){
			Info = null;
			return false;
		}
		// step 3: 真正去解析並包裝。
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

	[Doc($"""
#Sum[解析並包裝。]

#Params([[要解析的型別]])

#Rtn[包好的元資料；resolver 返回 null（未註冊）時為 null]

#Descr[
返回 null 由調用方記入負面緩存。

例：resolver 鏈裏的源生成上下文對未註冊型別返回 null，
本方法就跟著返回 null，不自己造一份假的官方元資料。
]
""")]
	private ITypeInfo? Build(Type Type){
		var JsonInfo = _resolver.GetTypeInfo(Type, _options);
		if(JsonInfo is null){
			return null;
		}
		return new JsonTypeInfoInfo(JsonInfo);
	}
}