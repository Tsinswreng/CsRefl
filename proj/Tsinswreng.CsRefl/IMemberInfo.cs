namespace Tsinswreng.CsRefl;

/// 型別上單個可訪問成員的元資料，是門面的最小單元。
///
/// 語義約定：
/// - CodeName 是「鍵」：CsSql 用它當列名，物件↔字典互轉用它當字典鍵。
/// - 反射來源的 CodeName 是 C# 成員名；JsonTypeInfo 來源的是 JSON 名。
///   「用 JsonTypeInfo 當來源」的成立前提是 JSON 名等於 C# 名
///   （由調用方保證命名策略為 null 且無 [JsonPropertyName]）。
/// - 非公開成員、靜態成員、索引器不進門面（兩套來源一致）。
/// - 讀寫動作的「前置檢查」：實例型別不符、不可讀/不可寫時 TryGet/TrySet 返回 false；
///   值本身型別不符的異常照常拋出，不吞（那是調用方的 bug，不是查詢失敗）。
public interface IMemberInfo{
	/// 成員名，即對外查詢、讀寫時使用的鍵。
	str CodeName{get;}
	/// JSON 名。僅 JsonTypeInfo 來源給出（恆等於它的 CodeName）；反射來源為 null。
	str? JsonName{get;}
	/// 成員種類：屬性 / 字段。
	EMemberKind Kind{get;}
	/// 成員宣告型別（屬性的 PropertyType / 字段的 FieldType）。
	Type DeclaredType{get;}
	/// 宣告本成員的型別。繼承成員的 DeclaringType 是基類，與實例的執行期型別不同。
	Type DeclaringType{get;}
	/// 本成員可讀（可作為字典值來源、可被 TryGet 讀取）。
	bool CanRead{get;}
	/// 本成員可寫（可被 TrySet 寫入）。
	bool CanWrite{get;}
	/// 成員序：兩套來源都保證穩定。
	/// 反射來源取 MetadataToken（即宣告序，基類在前）；JsonTypeInfo 來源取 Order 屬性。
	i32 Order{get;}

	/// 讀取實例上的本成員；實例型別不符或本成員不可讀時返回 false。
	bool TryGet(obj? O, out obj? R);
	/// 寫入實例上的本成員；實例型別不符或本成員不可寫時返回 false。
	bool TrySet(obj? O, obj? V);

	/// 本成員上的全部自定義特性（不含繼承的特性、不含內建元數據特性）。
	/// 反射來源直接取 GetCustomAttributes()；
	/// JsonTypeInfo 來源恆為空表（源生成下 AttributeProvider 走反射路徑，AOT 不可靠，
	/// 統一按「不支持」處理，這是兩套來源之間記錄在案的能力差別）。
	IReadOnlyList<Attribute> Attrs{get;}
	/// 按型別查本成員上的第一個該型別特性；沒有返回 false。
	bool TryGetAttr<TAttr>(out TAttr? Attr) where TAttr:Attribute;
}