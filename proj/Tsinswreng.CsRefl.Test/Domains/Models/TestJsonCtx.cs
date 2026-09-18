namespace Tsinswreng.CsRefl.Test.Domains.Models;

using System.Text.Json.Serialization;

/// 測試用源生成上下文：註冊本域要被 JsonTypeInfo 來源查詢的型別。
///
/// 命名策略為默認（null）且模型上沒有 [JsonPropertyName]，
/// 因此官方 JsonPropertyInfo.Name 與 C# 成員名一致，
/// 這正是本包把成員名當「可寫的 JSON 列名」使用的前提。
[JsonSerializable(typeof(PoUser))]
[JsonSerializable(typeof(PoUserBase))]
[JsonSerializable(typeof(PoUserExt))]
[JsonSerializable(typeof(PoColor))]
[JsonSerializable(typeof(List<str>))]
[JsonSerializable(typeof(Dictionary<str, i32>))]
public partial class TestJsonCtx:JsonSerializerContext{
}