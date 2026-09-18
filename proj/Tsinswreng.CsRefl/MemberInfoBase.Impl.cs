namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// MemberInfoBase 的函數實現。
/// 只放函數實現：成員事實字段與訪問器在 MemberInfoBase.cs。
public abstract partial class MemberInfoBase{
	/// 把派生類交出的官方成員對象與讀寫委託落地；事實一律在此一次性賦值。
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

	/// 實例型別是否合格：DeclaringType 已知時按它判定。
	/// 官方成員對象都不在（畸形手工元資料）時不阻擋，把判斷留給委託本身。
	private bool IsInstanceOk(obj O){
		var D = _declaringType;
		return D is null || D.IsInstanceOfType(O);
	}

	/// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可讀 → false。
	/// 動作本身失敗（極少見）包成 InvalidOperationException。
	public partial bool TryGet(obj? O, out obj? R){
		R = default;
		var GetFn = _get;
		if(GetFn is null || O is null || !IsInstanceOk(O)){
			return false;
		}
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

	/// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可寫 → false。
	/// 動作本身失敗（值型別不符、格式不符）統一包成 InvalidOperationException：
	/// 低層的 InvalidCastException/FormatException 對調用方不友好，也無法表達「哪個成員」。
	public partial bool TrySet(obj? O, obj? V){
		var SetFn = _set;
		if(SetFn is null || O is null || !IsInstanceOk(O)){
			return false;
		}
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