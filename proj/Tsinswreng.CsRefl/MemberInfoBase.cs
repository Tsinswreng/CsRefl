namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// 成員元資料的公共基類：把「成員事實」與「按名讀寫動作」集中在一處。
///
/// 存在的理由（代碼復用，不是抽象）：兩套來源的成員事實形狀完全一致，
/// TryGet/TrySet 的「實例型別 / 可讀寫性」前置檢查與異常映射也是同一套邏輯，
/// 放在這裡只寫一遍；兩套來源各自只交出官方成員對象與兩個讀寫委託。
/// 抽象維度仍由 IMemberInfo 接口承擔，本類不是抽象的替代品。
///
/// 與官方的最短距離：官方成員對象、官方成員名、官方宣告型別、官方成員種類
/// 全部原樣持有或直接轉發，不另存可能走樣的副本；只有兩側不共有的官方對象
/// （反射的 MemberInfo 與 Json 的 JsonPropertyInfo）各自留一個可空的官方出口。
/// 建構子與 TryGet/TrySet 的實現見 MemberInfoBase.Impl.cs。
public abstract partial class MemberInfoBase:IMemberInfo{
	/// 官方反射成員本體；JsonTypeInfo 來源為 null。
	private readonly global::System.Reflection.MemberInfo? _member;
	/// 官方 JSON 成員本體；反射來源為 null。
	private readonly JsonPropertyInfo? _json;
	/// 對外查詢、讀寫、字典鍵所用的名字（官方 MemberInfo/JsonPropertyInfo.Name）。
	private readonly str _name;
	/// 成員的宣告型別。
	private readonly Type _propertyType;
	/// 宣告本成員的型別。
	private readonly Type? _declaringType;
	/// 官方成員種類（MemberTypes）。
	private readonly MemberTypes _memberType;
	/// 本成員可否讀取。
	private readonly bool _canRead;
	/// 本成員可否寫入。
	private readonly bool _canWrite;
	/// 官方讀值委託（形狀與官方 JsonPropertyInfo.Get 一致）；null 表示不可讀。
	private readonly Func<obj, obj?>? _get;
	/// 官方寫值委託（形狀與官方 JsonPropertyInfo.Set 一致）；null 表示不可寫。
	private readonly Action<obj, obj?>? _set;
	/// 官方特性提供者（兩側共有的官方接口）。
	private readonly ICustomAttributeProvider? _attrProvider;

	/// 由派生類在自身建構子中把官方成員對象與讀寫委託一次交出；
	/// 事實只在這裡落地一次，派生類不再各自實現一批抽像屬性。
	protected partial MemberInfoBase(
		global::System.Reflection.MemberInfo? Member,
		JsonPropertyInfo? Json,
		str Name,
		Type PropertyType,
		Type? DeclaringType,
		MemberTypes MemberType,
		bool CanRead,
		bool CanWrite,
		Func<obj, obj?>? Get,
		Action<obj, obj?>? Set,
		ICustomAttributeProvider? AttributeProvider
	);

	/// 官方反射成員本體；JsonTypeInfo 來源為 null。
	public global::System.Reflection.MemberInfo? Member{
		get{
			return _member;
		}
	}
	/// 官方 JSON 成員本體；反射來源為 null。
	public JsonPropertyInfo? Json{
		get{
			return _json;
		}
	}
	public MemberTypes MemberType{
		get{
			return _memberType;
		}
	}
	public str Name{
		get{
			return _name;
		}
	}
	public Type PropertyType{
		get{
			return _propertyType;
		}
	}
	public Type? DeclaringType{
		get{
			return _declaringType;
		}
	}
	public bool CanRead{
		get{
			return _canRead;
		}
	}
	public bool CanWrite{
		get{
			return _canWrite;
		}
	}
	/// 官方讀值委託；不可讀為 null。
	public Func<obj, obj?>? Get{
		get{
			return _get;
		}
	}
	/// 官方寫值委託；不可寫為 null。
	public Action<obj, obj?>? Set{
		get{
			return _set;
		}
	}
	/// 官方特性提供者。
	public ICustomAttributeProvider? AttributeProvider{
		get{
			return _attrProvider;
		}
	}

	/// 讀取實例上的本成員；實例型別不符或本成員不可讀時返回 false。
	public partial bool TryGet(obj? O, out obj? R);
	/// 寫入實例上的本成員；實例型別不符或本成員不可寫時返回 false。
	public partial bool TrySet(obj? O, obj? V);
}