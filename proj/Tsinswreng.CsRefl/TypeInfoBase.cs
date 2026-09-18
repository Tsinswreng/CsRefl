namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;

/// 型別元資料的公共基類：把「按名查成員」與「名清單」這些只依賴 Members 的公共邏輯
/// 集中實現，兩套來源只需交出 Type/Kind/Members/鍵值型別/實例工廠/官方 JsonTypeInfo。
///
/// 存在的理由（代碼復用，不是抽象）：抽象維度由 ITypeInfo 接口承擔；
/// 本類不是它的替代品，只是把兩套來源共用的成員索引緩存寫一遍。
/// 建構子與按名查詢的實現見 TypeInfoBase.Impl.cs。
public abstract partial class TypeInfoBase:ITypeInfo{
	/// 本元資料對應的型別。
	private readonly Type _type;
	/// 型別分類（官方 JsonTypeInfoKind）。
	private readonly JsonTypeInfoKind _kind;
	/// 成員表（契約序，已去重：遮蔽成員只留最靠近實例的那份宣告）。
	private readonly IReadOnlyList<IMemberInfo> _members;
	/// 集合的元素型別；非集合為 null。
	private readonly Type? _elementType;
	/// 字典的鍵型別；非字典為 null。
	private readonly Type? _keyType;
	/// 按名的成員索引緩存，首次查詢時建立。
	private volatile Dictionary<str, IMemberInfo>? _byName;
	/// 可讀名清單緩存。
	private volatile IReadOnlyCollection<str>? _readable;
	/// 可寫名清單緩存。
	private volatile IReadOnlyCollection<str>? _writable;

	/// 由派生類交出型別事實；Members 會在此規整（見 TypeInfoBase.Impl.cs）。
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<IMemberInfo> Members,
		Type? ElementType,
		Type? KeyType
	);

	public Type Type{
		get{
			return _type;
		}
	}
	public JsonTypeInfoKind Kind{
		get{
			return _kind;
		}
	}
	public IReadOnlyList<IMemberInfo> Members{
		get{
			return _members;
		}
	}
	public Type? ElementType{
		get{
			return _elementType;
		}
	}
	public Type? KeyType{
		get{
			return _keyType;
		}
	}

	/// 無參實例工廠（官方 JsonTypeInfo.CreateObject 的形狀）；兩套來源各自提供。
	public abstract Func<obj>? CreateObject{get;}
	/// 被包裝的官方 JsonTypeInfo；反射來源為 null。兩套來源各自提供。
	public abstract JsonTypeInfo? Json{get;}

	/// 本型別能否建立無參實例：判據就是官方那條「CreateObject 是否為 null」。
	public bool CanMkInst{
		get{
			return CreateObject is not null;
		}
	}
	/// 建立一個無參實例（轉調 CreateObject）；不可建時拋 NotSupportedException。
	public abstract obj? MkInst();

	/// 可讀成員名清單，順序同 Members（已去重，不含重複名）。
	public IReadOnlyCollection<str> ReadableNames{
		get{
			return _readable ??= Members.Where(M => M.CanRead).Select(M => M.Name).ToList();
		}
	}
	/// 可寫成員名清單，順序同 Members（已去重，不含重複名）。
	public IReadOnlyCollection<str> WritableNames{
		get{
			return _writable ??= Members.Where(M => M.CanWrite).Select(M => M.Name).ToList();
		}
	}

	/// 按名查成員；未知返回 false。
	/// 參數特性（NotNullWhen）只寫在聲明側；partial 合併時兩邊都標會報 CS0579。
	public partial bool TryGetMember(str Name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IMemberInfo? M);
	/// 按名取成員；未知拋 KeyNotFoundException，訊息含可用名清單。
	public partial IMemberInfo GetMember(str Name);
}