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

實測：查 `PoUser` 的 `Age`，反射源的建構子只把 {nameof(PropertyInfo)} 的
{nameof(Name)}（"Age"）、{nameof(PropertyType)}（`typeof(i32)`）、
{nameof(DeclaringType)}（`typeof(PoUser)`）與兩個委託交出來；
{nameof(JsonMemberInfo)} 交的是 {nameof(JsonPropertyInfo)} 的對應物，也是同一批事實；
之後的前置檢查與異常映射兩邊完全共用，故 {nameof(TryGet)} 的行為兩套來源一字不差。
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
實測：查 `PoUser` 的 `Age`，反射源這個字段是一個 {nameof(PropertyInfo)}、
Json 源為 null；查 `Note` 時反射源是一個 {nameof(FieldInfo)}；
Json 源恆為 null，因為官方 {nameof(JsonPropertyInfo)} 不是 {nameof(System.Reflection.MemberInfo)} 的子類。
]
""")]
	private readonly global::System.Reflection.MemberInfo? _member;

	[Doc($"""
#Sum[官方 JSON 成員本體；反射來源為 null。]

#Descr[
實測：Json 源包 `PoUser.Age` 時這個字段非 null，
可取官方獨有能力（`{nameof(JsonPropertyInfo.IsRequired)}` 等）；
反射源的同一個成員這裡是 null。
]
""")]
	private readonly JsonPropertyInfo? _json;

	[Doc($"""
#Sum[對外查詢、讀寫、字典鍵所用的名字。]

#Descr[
即官方 {nameof(System.Reflection.MemberInfo.Name)}／{nameof(JsonPropertyInfo.Name)}，不另存副本。

實測：成員 `Age` 的這個字段就是 "Age"、`Note` 的就是 "Note"；
{nameof(ITypeInfo.GetMember)}("Age") 命的也是它。
]
""")]
	private readonly str _name;

	[Doc($"""
#Sum[成員的型別（官方 {nameof(JsonPropertyInfo.PropertyType)} 同義）。]

#Descr[
實測（`PoUser`）：`Age` 的這個字段是 `typeof(i32)`、`Secret` 是 `typeof(str)`、
`Tags` 是 `typeof(List<str>)`；
繼承成員的值取自宣告它的那個屬性本身，與實例無關。
]
""")]
	private readonly Type _propertyType;

	[Doc($"""
#Sum[宣告本成員的型別。]

#Descr[
實測（`PoUser` 繼承 `PoUserBase`）：`Id` 的這個字段是 `typeof(PoUserBase)`、`Age` 的是 `typeof(PoUser)`；
官方 {nameof(JsonPropertyInfo.DeclaringType)} 可能為 null，那情形也照官方轉發。
]
""")]
	private readonly Type? _declaringType;

	[Doc($"""
#Sum[官方成員種類（{nameof(MemberTypes)}）。]

#Descr[
實測（`PoUser`）：反射源的 `Age` 是 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}、
`Note`（`[JsonInclude]` 字段）是 {nameof(MemberTypes)}.{nameof(MemberTypes.Field)}；
Json 源一律 {nameof(MemberTypes)}.{nameof(MemberTypes.Property)}（官方不暴露字段這一事實）。
]
""")]
	private readonly MemberTypes _memberType;

	[Doc($"""
#Sum[本成員可否讀取。]

#Descr[
實測：只寫的 `Token` 為 false（它讀不出來），此時 {nameof(IMemberInfo.Get)} 也是 null；
只讀的 `Secret` 為 true，其 {nameof(IMemberInfo.Get)} 非 null，兩者恆同步。
]
""")]
	private readonly bool _canRead;

	[Doc($"""
#Sum[本成員可否寫入。]

#Descr[
實測：只讀的 `Secret` 為 false（它寫不進去），此時 {nameof(IMemberInfo.Set)} 也是 null；
只寫的 `Token` 為 true，其 {nameof(IMemberInfo.Set)} 非 null，兩者恆同步。
]
""")]
	private readonly bool _canWrite;

	[Doc($"""
#Sum[官方讀值委託（形狀與官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)} 一致）；null 表示不可讀。]

#Descr[
實測：反射源的這個委託內部就是 {nameof(PropertyInfo.GetValue)} 與 {nameof(FieldInfo.GetValue)}，
`Age` 的委託讀出 boxed 的 `i32` 30；
Json 源直接拿官方源生成的委託，故 AOT 下讀值不走反射。
]
""")]
	private readonly Func<obj, obj?>? _get;

	[Doc($"""
#Sum[官方寫值委託（形狀與官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)} 一致）；null 表示不可寫。]

#Descr[
實測：反射源的這個委託內部就是 {nameof(PropertyInfo.SetValue)} 與 {nameof(FieldInfo.SetValue)}，
`Age` 的委託把 31 寫進實例；
只讀的 `Secret` 這個字段為 null，{nameof(TrySet)} 在此之前就返回 false，不會撞到異常。
]
""")]
	private readonly Action<obj, obj?>? _set;

	[Doc($"""
#Sum[官方特性提供者（兩側共有的官方接口）。]

#Descr[
實測：反射源這裡就是成員自身（`Age` 時即那個 {nameof(PropertyInfo)}）；
Json 源這裡是官方 {nameof(JsonPropertyInfo.AttributeProvider)}；
兩者都能用 {nameof(IMemberInfoExtn.GetCustomAttribute)} 取特性，
`PoUser.Level` 上兩套來源都取回 1 個 `MyDemoAttr`。
]
""")]
	private readonly ICustomAttributeProvider? _attrProvider;

	[Doc($"""
#Sum[由派生類交出官方成員對象與讀寫委託，建構子內一次落地全部成員事實。]

#Params([
	[Member, 官方反射成員本體；{nameof(JsonTypeInfoSrc)} 來源傳 null],
	[Json, 官方 JSON 成員本體；反射來源傳 null],
	[Name, 成員名],
	[PropertyType, 成員的型別],
	[DeclaringType, 宣告本成員的型別],
	[MemberType, 官方成員種類],
	[CanRead, 本成員可否讀取],
	[CanWrite, 本成員可否寫入],
	[Get, 官方讀值委託；不可讀傳 null],
	[Set, 官方寫值委託；不可寫傳 null],
	[AttributeProvider, 官方特性提供者]
])

#Descr[
事實只在這裡落地一次，派生類不再各自實現一批抽像屬性。

實測：{nameof(ReflMemberInfo)} 與 {nameof(JsonMemberInfo)} 都交 11 個實參，
差別只有 {nameof(Member)} 與 {nameof(Json)} 哪個為 null；
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
實測：{nameof(ReflMemberInfo)} 建的 `Age` 成員這個屬性就是那個 {nameof(PropertyInfo)}，
可直接調官方反射能力（例如 `GetCustomAttributes`）；
Json 源的同名成員這裡是 null。
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
實測：要問 `PoUser.Age` 這個成員是否必填，官方能力在 {nameof(Json)} 上，
故 `M.{nameof(Json)}?.{nameof(JsonPropertyInfo.IsRequired)}` 得到 `bool?`；
反射源那個 `M` 上這個式子是 null，說明當前沒有這個信息。
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

	// ---- 私有輔助（實現見 MemberInfoBase.Impl.cs）----

	[Doc($"""
#Sum[實例型別是否合格。]

#Params([[O, 待檢查的實例]])

#Rtn[合格返回 true]

#Descr[
{nameof(DeclaringType)} 已知時按它判定；
官方成員對象都不在（畸形手工元資料）時不阻擋，把判斷留給委託本身。

實測：用 `PoUser` 的元資料建的成員 `Age`，
傳 `new PoUser()` 返回 true、傳 `new PoColor()` 返回 false（型別不符）；
繼承成員 `Id`（宣告於 `PoUserBase`）傳子類 `PoUser` 實例也返回 true，
故基類成員能讀寫子類實例，這是繼承場景的正常用法。
]
""")]
	private partial bool IsInstanceOk(obj O);
}