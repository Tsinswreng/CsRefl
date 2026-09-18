namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// JsonTypeInfo 來源的型別元資料：包一個非泛型 JsonTypeInfo。
/// 官方本體直接對外暴露（Json），需要甚麼官方能力直接從那裡拿；
/// Kind 直接就是官方 JsonTypeInfoKind，無需映射。
/// 成員沿用 JsonTypeInfo.Properties 的既有序。
/// 建構子與 MkInst 實現見 JsonTypeInfoInfo.Impl.cs。
public partial class JsonTypeInfoInfo:TypeInfoBase{
	/// 被包的官方 JsonTypeInfo。
	private readonly JsonTypeInfo _json;

	/// 包一個 JsonTypeInfo。
	public partial JsonTypeInfoInfo(JsonTypeInfo Json);

	/// 無參實例工廠，直接轉官方 JsonTypeInfo.CreateObject；
	/// 官方就是用它表示「可不可以建實例」（標量等型別官方給 null）。
	public override Func<obj>? CreateObject{
		get{
			return _json.CreateObject;
		}
	}

	/// 被包的官方 JsonTypeInfo 本體。
	public override JsonTypeInfo Json{
		get{
			return _json;
		}
	}

	/// 建立實例，轉調 CreateObject 工廠；無無參工廠時拋 NotSupportedException。
	public override partial obj? MkInst();
}