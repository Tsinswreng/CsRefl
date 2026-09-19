namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[成員元資料的公共基類：把「成員事實」與「按名讀寫動作」集中在一處。]

#Descr[
存在的理由（代碼復用，不是抽象）：
兩套來源的成員事實形狀完全一致，
{nameof(TryGet)}／{nameof(TrySet)} 的「實例型別、可讀寫性」前置檢查與異常映射也是同一套邏輯，
放在這裡只寫一遍；
兩套來源各自只交出官方成員對象與兩個讀寫委託。
抽象維度仍由 {nameof(IMemberInfo)} 接口承擔，本類不是抽象的替代品。

例：{nameof(ReflMemberInfo)} 的建構子只做「把 {nameof(PropertyInfo)} 的
{nameof(Name)}、{nameof(PropertyType)}、{nameof(DeclaringType)} 與兩個委託交出來」這一件事，
{nameof(JsonMemberInfo)} 交的是 {nameof(JsonPropertyInfo)} 的對應物，
之後的檢查與異常處理兩邊完全共用。
]

#Descr[
與官方的最短距離：
官方成員對象、官方成員名、官方宣告型別、官方成員種類
全部原樣持有或直接轉發，不另存可能走樣的副本；
只有兩側不共有的官方對象
（反射的 {nameof(System.Reflection.MemberInfo)} 與 Json 的 {nameof(JsonPropertyInfo)}）
各自留一個可空的官方出口。

{nameof(IMemberInfo)} 那一組屬性與 {nameof(TryGet)}／{nameof(TrySet)} 的說明見接口；
此處只寫本類新增的聲明。

建構子與 {nameof(TryGet)}／{nameof(TrySet)} 的實現見 `MemberInfoBase.Impl.cs`。
]
""")]
public abstract partial class MemberInfoBase:IMemberInfo{
	[Doc($"""
#Sum[官方反射成員本體；{nameof(JsonTypeInfoSrc)} 來源為 null。]

#Descr[
例：反射源包一個屬性時這裡是 {nameof(PropertyInfo)}，包一個字段時是 {nameof(FieldInfo)}；
Json 源恆為 null，因為官方 {nameof(JsonPropertyInfo)} 不是 {nameof(System.Reflection.MemberInfo)} 的子類。
]
""")]
	private readonly global::System.Reflection.MemberInfo? _member;

	[Doc($"""
#Sum[官方 JSON 成員本體；反射來源為 null。]

#Descr[
例：Json 源包一個成員時這裡非 null，可取官方獨有能力
（{nameof(JsonPropertyInfo.IsRequired)} 等）；反射源恆為 null。
]
""")]
	private readonly JsonPropertyInfo? _json;

	[Doc($"""
#Sum[對外查詢、讀寫、字典鍵所用的名字。]

#Descr[
即官方 {nameof(System.Reflection.MemberInfo.Name)}／{nameof(JsonPropertyInfo.Name)}，不另存副本。

例：成員 `Age` 的這個字段就是 "Age"，
{nameof(ITypeInfo.GetMember)}("Age") 命的也是它。
]
""")]
	private readonly str _name;

	[Doc($"""
#Sum[成員的型別（官方 {nameof(JsonPropertyInfo.PropertyType)} 同義）。]

#Descr[
例：`i32 Age` 屬性是 `typeof(i32)`；
繼承成員的這個值取自宣告它的那個屬性本身，與實例無關。
]
""")]
	private readonly Type _propertyType;

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
例：繼承成員的這個值是基類，不是實例的執行期型別；
拿基類 {nameof(JsonPropertyInfo.DeclaringType)} 可能為 null 的情形也照官方轉發。
]
""")]
	private readonly Type? _declaringType;

	[Doc($"""
#Sum[官方成員種類（{nameof(MemberTypes)}）。]

#Descr[
例：反射源的屬性成員是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}，
字段成員是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
Json 源一律 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}。
]
""")]
	private readonly MemberTypes _memberType;

	[Doc($"""
#Sum[本成員可否讀取。]

#Descr[
例：只寫屬性為 false，此時 {nameof(IMemberInfo.Get)} 也是 null，兩者恆同步。
]
""")]
	private readonly bool _canRead;

	[Doc($"""
#Sum[本成員可否寫入。]

#Descr[
例：只讀屬性為 false，此時 {nameof(IMemberInfo.Set)} 也是 null。
]
""")]
	private readonly bool _canWrite;

	[Doc($"""
#Sum[官方讀值委託（形狀與官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)} 一致）；null 表示不可讀。]

#Descr[
例：反射源的委託內部就是 {nameof(PropertyInfo.GetValue)}；
Json 源直接拿官方源生成的委託，故 AOT 下讀值不走反射。
]
""")]
	private readonly Func<obj, obj?>? _get;

	[Doc($"""
#Sum[官方寫值委託（形狀與官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)} 一致）；null 表示不可寫。]

#Descr[
例：反射源的委託內部就是 {nameof(PropertyInfo.SetValue)}；
寫只讀成員之前 {nameof(TrySet)} 會先看這個是否為 null 並直接返回 false，不會撞到異常。
]
""")]
	private readonly Action<obj, obj?>? _set;

	[Doc($"""
#Sum[官方特性提供者（兩側共有的官方接口）。]

#Descr[
例：反射源這裡就是成員自身；
Json 源這裡是官方 {nameof(JsonPropertyInfo.AttributeProvider)}，
兩者都能用 {nameof(IMemberInfoExtn.GetCustomAttribute)} 取特性。
]
""")]
	private readonly ICustomAttributeProvider? _attrProvider;

	[Doc($"""
#Sum[由派生類交出官方成員對象與讀寫委託，建構子內一次落地全部成員事實。]

#Params([
	[官方反射成員本體；{nameof(JsonTypeInfoSrc)} 來源傳 null],
	[官方 JSON 成員本體；反射來源傳 null],
	[成員名],
	[成員的型別],
	[宣告本成員的型別],
	[官方成員種類],
	[本成員可否讀取],
	[本成員可否寫入],
	[官方讀值委託；不可讀傳 null],
	[官方寫值委託；不可寫傳 null],
	[官方特性提供者]
])

#Descr[
事實只在這裡落地一次，派生類不再各自實現一批抽像屬性。

例：{nameof(ReflMemberInfo)} 交 11 個實參、
{nameof(JsonMemberInfo)} 也交 11 個（只是 {nameof(Member)} 與 {nameof(Json)} 哪個為 null 不同），
派生類因此都只有建構子，沒有額外成員。
]
""")]
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

	[Doc($"""
#Sum[官方反射成員本體；{nameof(JsonTypeInfoSrc)} 來源為 null。]

#Descr[
例：`M is {nameof(IMemberInfo)}` 之後，`(M as {nameof(ReflMemberInfo)})` 取得的物件上
這個屬性就是成員的官方本體，可直接調官方反射能力。
]

#See[{nameof(IMemberInfo.Member)}]
""")]
	public global::System.Reflection.MemberInfo? Member{
		get{
			return _member;
		}
	}

	[Doc($"""
#Sum[官方 JSON 成員本體；反射來源為 null。]

#Descr[
例：要問「這個成員是否必填」，官方能力在 {nameof(Json)} 上，
故 `M.{nameof(Json)}?.{nameof(JsonPropertyInfo.IsRequired)}` 得到 `bool?`，
null 就說明當前是反射源、沒有這個信息。
]

#See[{nameof(IMemberInfo.Json)}]
""")]
	public JsonPropertyInfo? Json{
		get{
			return _json;
		}
	}

	[Doc($"""
#Sum[成員的官方元資料種類。]

#See[{nameof(IMemberInfo.MemberType)}]
""")]
	public MemberTypes MemberType{
		get{
			return _memberType;
		}
	}

	[Doc($"""
#Sum[成員名，即對外查詢、讀寫時使用的鍵。]

#See[{nameof(IMemberInfo.Name)}]
""")]
	public str Name{
		get{
			return _name;
		}
	}

	[Doc($"""
#Sum[成員的型別。]

#See[{nameof(IMemberInfo.PropertyType)}]
""")]
	public Type PropertyType{
		get{
			return _propertyType;
		}
	}

	[Doc($"""
#Sum[宣告本成員的型別。]

#See[{nameof(IMemberInfo.DeclaringType)}]
""")]
	public Type? DeclaringType{
		get{
			return _declaringType;
		}
	}

	[Doc($"""
#Sum[本成員可讀。]

#See[{nameof(IMemberInfo.CanRead)}]
""")]
	public bool CanRead{
		get{
			return _canRead;
		}
	}

	[Doc($"""
#Sum[本成員可寫。]

#See[{nameof(IMemberInfo.CanWrite)}]
""")]
	public bool CanWrite{
		get{
			return _canWrite;
		}
	}

	[Doc($"""
#Sum[官方讀值委託；不可讀為 null。]

#See[{nameof(IMemberInfo.Get)}]
""")]
	public Func<obj, obj?>? Get{
		get{
			return _get;
		}
	}

	[Doc($"""
#Sum[官方寫值委託；不可寫為 null。]

#See[{nameof(IMemberInfo.Set)}]
""")]
	public Action<obj, obj?>? Set{
		get{
			return _set;
		}
	}

	[Doc($"""
#Sum[官方特性提供者。]

#See[{nameof(IMemberInfo.AttributeProvider)}]
""")]
	public ICustomAttributeProvider? AttributeProvider{
		get{
			return _attrProvider;
		}
	}

	[Doc($"""
#Sum[讀取實例上的本成員。]

#See[{nameof(IMemberInfo.TryGet)}]
""")]
	public partial bool TryGet(obj? O, out obj? R);

	[Doc($"""
#Sum[寫入實例上的本成員。]

#See[{nameof(IMemberInfo.TrySet)}]
""")]
	public partial bool TrySet(obj? O, obj? V);
}