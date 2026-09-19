namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 測試模型：一網打盡要驗證的成員形態。
///
/// 期望（兩套來源一致）：
/// - 公開實例屬性全收：Id/Name（繼承）→ Age/Email/Married/Tags/Extra/Secret/Level/Token；
/// - [JsonInclude] 公開字段 Note 兩套來源都有；
/// - 只讀屬性 Secret 收錄但 CanWrite=false；
/// - 只寫屬性 Token 收錄但 CanRead=false（它在字典視圖裏「寫得進、讀不到、不是鍵」）；
/// - 靜態成員 StaticNote、私有字段 Hidden、索引器（this[i32]）一律不收。
///
/// 成員序：宣告序（基類在前），即 Id、Name、Age、Email、Married、Tags、Extra、
/// Secret、Level、Token、Note。
public class PoUser:PoUserBase{
	/// 年齡。
	public i32 Age{get;set;}
	/// 郵箱（可為空）。
	public str? Email{get;set;}
	/// 已婚與否。
	public bool Married{get;set;}
	/// 標籤列表（集合成員）。
	public List<str> Tags{get;set;} = [];
	/// 擴展字典（字典成員）。
	public Dictionary<str, i32> Extra{get;set;} = [];
	/// 只讀屬性：可讀不可寫。
	public str Secret{get;} = "s";
	/// 靜態成員：不進門面。
	public static str StaticNote{get;set;} = "st";
	/// 私有屬性：不進門面（用屬性形狀是為了不觸發未使用字段警告；
/// 排除規則同樣覆蓋私有屬性與私有字段）。
	private str Hidden{get;} = "h";
	/// 索引器：不進門面。
	public str this[i32 Index] => $"i{Index}";
	/// 帶特性的屬性：兩套來源都能透過 AttributeProvider 查到 MyDemoAttr
	/// （實測源生成下也取得到，故不是「只有反射查得到」）。
	[MyDemoAttr("優等級", 2)]
	public i32 Level{get;set;}
	/// 只寫屬性：對外可寫不可讀——get 是私有的，故 PropertyInfo.CanRead=false
/// （驗證字典視圖「寫得進但不在鍵表」的一側）。私有 get 讓測試仍能斷言寫入結果，
	/// 而它不影響公開面。
	public str Token{
		private get{
			return _token;
		}
		set{
			_token = value;
		}
	}
	/// Token 的存放處。
	private str _token = "";
	/// 供測試斷言「Token 是否真的寫進去了」的內部讀取面。
	/// 用 internal 而非 public：反射源的成員表只收公開成員，
	/// 這個屬性不會污染本模型對外承諾的成員序（見本類開頭的期望清單）。
	internal str TokenEcho{
		get{
			return _token;
		}
	}
	/// [JsonInclude] 公開字段：兩套來源都收（Json 源對字段只能標 Property）。
	[System.Text.Json.Serialization.JsonInclude]
	public str Note = "n";
}