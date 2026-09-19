namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(MemberInfoBase)} 的函數實現。]

#Descr[
只放函數實現：成員事實字段與訪問器在 `MemberInfoBase.cs`。
]
""")]
public abstract partial class MemberInfoBase{
	[Doc($"""
#Sum[把派生類交出的官方成員對象與讀寫委託落地；事實一律在此一次性賦值。]

#Descr[
例：{nameof(ReflMemberInfo)} 交的是 {nameof(PropertyInfo)} 側的事實，
{nameof(JsonMemberInfo)} 交的是 {nameof(JsonPropertyInfo)} 側的事實，
本建構子只做賦值，不含分支（分支留給各自的派生類建構子）。
]

#See[{nameof(MemberInfoBase)}]
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
	){
		_member = Member;
		_json = Json;
		_name = Name;
		_propertyType = PropertyType;
		_declaringType = DeclaringType;
		_memberType = MemberType;
		_canRead = CanRead;
		_canWrite = CanWrite;
		_get = Get;
		_set = Set;
		_attrProvider = AttributeProvider;
	}

	[Doc($"""
#Sum[實例型別是否合格。]

#Params([[待檢查的實例]])

#Rtn[合格返回 true]

#Descr[
{nameof(IMemberInfo.DeclaringType)} 已知時按它判定。
官方成員對象都不在（畸形手工元資料）時不阻擋，把判斷留給委託本身。

例：成員宣告在基類、實例是子類時判定為合格，
故用基類元資料建的成員能讀寫子類實例，這是繼承場景的正常用法。
]
""")]
	private bool IsInstanceOk(obj O){
		var D = _declaringType;
		return D is null || D.IsInstanceOfType(O);
	}

	[Doc($"""
#Sum[讀取實例上的本成員。]

#Descr[
例：`M.{nameof(TryGet)}(User, out var V)` 命中；
`M.{nameof(TryGet)}(null, out _)` 返回 false（不拋 {nameof(NullReferenceException)}）；
拿另一種型別的實例返回 false；
讀只寫成員返回 false。
]

#See[{nameof(IMemberInfo.TryGet)}]
""")]
	public partial bool TryGet(obj? O, out obj? R){
		// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可讀 → false。
		R = default;
		var GetFn = _get;
		if(GetFn is null || O is null || !IsInstanceOk(O)){
			return false;
		}
		// 動作本身失敗（極少見）包成 InvalidOperationException，不讓低層異常裸奔。
		try{
			R = GetFn(O);
			return true;
		}
		catch(InvalidCastException E){
			throw new InvalidOperationException($"讀取 {_declaringType?.FullName}.{Name} 失敗（值型別不符）。", E);
		}
		catch(ArgumentException E){
			throw new InvalidOperationException($"讀取 {_declaringType?.FullName}.{Name} 失敗。", E);
		}
	}

	[Doc($"""
#Sum[寫入實例上的本成員。]

#Descr[
例：`M.{nameof(TrySet)}(User, 31)` 命中並寫回；
寫只讀成員返回 false（不改動實例、不拋）；
傳錯型別的實例返回 false；
值型別不符（如拿 `str` 當 `i32` 寫）包成 {nameof(InvalidOperationException)}，
訊息含成員全名、值型別名與成員型別名，故不必再自己去比對型別。
]

#See[{nameof(IMemberInfo.TrySet)}]
""")]
	public partial bool TrySet(obj? O, obj? V){
		// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可寫 → false。
		var SetFn = _set;
		if(SetFn is null || O is null || !IsInstanceOk(O)){
			return false;
		}
		// 動作本身失敗（值型別不符、格式不符）統一包成 InvalidOperationException：
		// 低層的 InvalidCastException/FormatException 對調用方不友好，也無法表達「哪個成員」。
		try{
			SetFn(O, V);
			return true;
		}
		catch(InvalidCastException E){
			throw new InvalidOperationException($"寫入 {_declaringType?.FullName}.{Name} 失敗：值 {V?.GetType().Name} 與成員型別 {PropertyType.Name} 不符。", E);
		}
		catch(ArgumentException E){
			throw new InvalidOperationException($"寫入 {_declaringType?.FullName}.{Name} 失敗：值型別不符。", E);
		}
		catch(FormatException E){
			throw new InvalidOperationException($"寫入 {_declaringType?.FullName}.{Name} 失敗：值無法轉換（格式不符）。", E);
		}
	}
}