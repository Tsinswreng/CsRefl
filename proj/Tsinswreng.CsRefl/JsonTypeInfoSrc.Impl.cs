namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// JsonTypeInfoSrc 的函數實現。
public partial class JsonTypeInfoSrc{
	/// 用一個源生成 context 建來源（最常見：AppJsonCtx.Default）。
	public JsonTypeInfoSrc(JsonSerializerContext Ctx){
		ArgumentNullException.ThrowIfNull(Ctx);
		// JsonSerializerContext 顯式實現 IJsonTypeInfoResolver，轉介面調用。
		_resolver = Ctx;
		_options = Ctx.Options;
	}

	/// 用一份配好的 options 建來源（其 TypeInfoResolver 鏈裏含源生成 context 即可）。
	public JsonTypeInfoSrc(JsonSerializerOptions Options){
		ArgumentNullException.ThrowIfNull(Options);
		_resolver = Options.TypeInfoResolver
			?? throw new ArgumentException(
				"Options.TypeInfoResolver 為空；JsonTypeInfo 來源需要一個能解析型別的 resolver（如源生成 context）。",
				nameof(Options)
			);
		_options = Options;
	}

	/// 取已註冊型別的元資料；未註冊返回 false。不支持列舉（RegisteredTypes 為 null）。
	public bool TryGetInfo(
		// DAM 註解與接口一致（本來源不依賴成員元數據，但接口統一宣告前置條件）。
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	){
		ArgumentNullException.ThrowIfNull(Type);
		if(_cache.TryGetValue(Type, out Info)){
			return true;
		}
		Info = Build(Type);
		if(Info is null){
			return false;
		}
		// 並行下重複 TryAdd 無害（包的是同一 JsonTypeInfo 實例）。
		_cache.TryAdd(Type, Info);
		return true;
	}

	/// 解析並包裝；resolver 返回 null（未註冊）時返回 null，不進緩存。
	private ITypeInfo? Build(Type Type){
		var JsonInfo = _resolver.GetTypeInfo(Type, _options);
		if(JsonInfo is null){
			return null;
		}
		return new JsonTypeInfoInfo(JsonInfo);
	}
}