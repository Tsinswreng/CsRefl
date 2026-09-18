namespace Tsinswreng.CsRefl;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[型別元資料的公共基類：把只依賴 `Members` 的公共邏輯集中實現。]

#Descr[
集中起來的是「按名查成員」與「名清單」這些只依賴 `Members` 的邏輯；
兩套來源只需交出
`Type`／`Kind`／`Members`／鍵值型別／實例工廠／官方 `JsonTypeInfo`。
]

#Descr[
存在的理由（代碼復用，不是抽象）：
抽象維度由 `ITypeInfo` 接口承擔；
本類不是它的替代品，只是把兩套來源共用的成員索引緩存寫一遍。

`ITypeInfo` 那一組屬性與按名查詢的說明見接口；
此處只寫本類新增的聲明。

建構子與按名查詢的實現見 `TypeInfoBase.Impl.cs`。
]
""")]
public abstract partial class TypeInfoBase:ITypeInfo{
	[Doc("""
#Sum[本元資料對應的型別。]
""")]
	private readonly Type _type;

	[Doc("""
#Sum[型別分類（官方 `JsonTypeInfoKind`）。]
""")]
	private readonly JsonTypeInfoKind _kind;

	[Doc("""
#Sum[成員表（契約序，已去重：遮蔽成員只留最靠近實例的那份宣告）。]
""")]
	private readonly IReadOnlyList<IMemberInfo> _members;

	[Doc("""
#Sum[集合的元素型別；非集合為 null。]
""")]
	private readonly Type? _elementType;

	[Doc("""
#Sum[字典的鍵型別；非字典為 null。]
""")]
	private readonly Type? _keyType;

	[Doc("""
#Sum[按名的成員索引緩存，首次查詢時建立。]
""")]
	private volatile Dictionary<str, IMemberInfo>? _byName;

	[Doc("""
#Sum[可讀名清單緩存。]
""")]
	private volatile IReadOnlyCollection<str>? _readable;

	[Doc("""
#Sum[可寫名清單緩存。]
""")]
	private volatile IReadOnlyCollection<str>? _writable;

	[Doc("""
#Sum[由派生類交出型別事實；`Members` 會在此規整。]

#Params([
	[本元資料對應的型別],
	[型別分類（官方 `JsonTypeInfoKind`）],
	[成員表；此處會排序去重成契約序],
	[集合的元素型別；非集合傳 null],
	[字典的鍵型別；非字典傳 null]
])

#See[{nameof(TypeInfoBase)}]
""")]
	protected partial TypeInfoBase(
		Type Type,
		JsonTypeInfoKind Kind,
		IReadOnlyList<IMemberInfo> Members,
		Type? ElementType,
		Type? KeyType
	);

	[Doc("""
#Sum[本元資料對應的型別。]

#See[{nameof(ITypeInfo.Type)}]
""")]
	public Type Type{
		get{
			return _type;
		}
	}

	[Doc("""
#Sum[型別分類，直接用官方 `JsonTypeInfoKind`。]

#See[{nameof(ITypeInfo.Kind)}]
""")]
	public JsonTypeInfoKind Kind{
		get{
			return _kind;
		}
	}

	[Doc("""
#Sum[全部成員，順序 = 契約序。]

#See[{nameof(ITypeInfo.Members)}]
""")]
	public IReadOnlyList<IMemberInfo> Members{
		get{
			return _members;
		}
	}

	[Doc("""
#Sum[集合的元素型別；非集合為 null。]

#See[{nameof(ITypeInfo.ElementType)}]
""")]
	public Type? ElementType{
		get{
			return _elementType;
		}
	}

	[Doc("""
#Sum[字典的鍵型別；非字典為 null。]

#See[{nameof(ITypeInfo.KeyType)}]
""")]
	public Type? KeyType{
		get{
			return _keyType;
		}
	}

	[Doc("""
#Sum[無參實例工廠（官方 `JsonTypeInfo.CreateObject` 的形狀）；兩套來源各自提供。]

#See[{nameof(ITypeInfo.CreateObject)}]
""")]
	public abstract Func<obj>? CreateObject{get;}

	[Doc("""
#Sum[被包裝的官方 `JsonTypeInfo`；反射來源為 null。兩套來源各自提供。]

#See[{nameof(ITypeInfo.Json)}]
""")]
	public abstract JsonTypeInfo? Json{get;}

	[Doc("""
#Sum[本型別能否建立無參實例。]

#See[{nameof(ITypeInfo.CanMkInst)}]
""")]
	public bool CanMkInst{
		get{
			return CreateObject is not null;
		}
	}

	[Doc("""
#Sum[建立一個無參實例（轉調 `CreateObject`）；不可建時拋 `NotSupportedException`。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public abstract obj? MkInst();

	[Doc("""
#Sum[可讀成員名清單，順序同 `Members`（已去重，不含重複名）。]

#See[{nameof(ITypeInfo.ReadableNames)}]
""")]
	public IReadOnlyCollection<str> ReadableNames{
		get{
			return _readable ??= Members.Where(M => M.CanRead).Select(M => M.Name).ToList();
		}
	}

	[Doc("""
#Sum[可寫成員名清單，順序同 `Members`（已去重，不含重複名）。]

#See[{nameof(ITypeInfo.WritableNames)}]
""")]
	public IReadOnlyCollection<str> WritableNames{
		get{
			return _writable ??= Members.Where(M => M.CanWrite).Select(M => M.Name).ToList();
		}
	}

	[Doc("""
#Sum[按名查成員；未知返回 false。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IMemberInfo? M);

	[Doc("""
#Sum[按名取成員；未知拋 `KeyNotFoundException`，訊息含可用名清單。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name);
}