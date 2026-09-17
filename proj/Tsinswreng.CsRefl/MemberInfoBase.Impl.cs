namespace Tsinswreng.CsRefl;

/// MemberInfoBase 的函數實現。
public abstract partial class MemberInfoBase{
	/// 用讀寫委託建構；事實（CodeName 等）由派生類在建構子裏逐個賦值給抽象屬性。
	protected MemberInfoBase(Func<obj, obj?>? GetFn, Action<obj, obj?>? SetFn){
		_getFn = GetFn;
		_setFn = SetFn;
	}

	/// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可讀 → false。
	/// 動作本身失敗（極少見）包成 InvalidOperationException。
	public bool TryGet(obj? O, out obj? R){
		R = default;
		var Get = _getFn;
		if(Get is null || O is null || !DeclaringType.IsInstanceOfType(O)){
			return false;
		}
		try{
			R = Get(O!);
			return true;
		}
		catch(InvalidCastException E){
			throw new InvalidOperationException($"讀取 {DeclaringType.FullName}.{CodeName} 失敗（值型別不符）。", E);
		}
		catch(ArgumentException E){
			throw new InvalidOperationException($"讀取 {DeclaringType.FullName}.{CodeName} 失敗。", E);
		}
	}

	/// 前置檢查：實例為 null、不是 DeclaringType 的實例、或不可寫 → false。
	/// 動作本身失敗（值型別不符、格式不符）統一包成 InvalidOperationException：
	/// 低層的 InvalidCastException/FormatException 對調用方不友好，也無法表達「哪個成員」。
	public bool TrySet(obj? O, obj? V){
		var Set = _setFn;
		if(Set is null || O is null || !DeclaringType.IsInstanceOfType(O)){
			return false;
		}
		try{
			Set(O!, V);
			return true;
		}
		catch(InvalidCastException E){
			throw new InvalidOperationException($"寫入 {DeclaringType.FullName}.{CodeName} 失敗：值 {V?.GetType().Name} 與成員型別 {DeclaredType.Name} 不符。", E);
		}
		catch(ArgumentException E){
			throw new InvalidOperationException($"寫入 {DeclaringType.FullName}.{CodeName} 失敗：值型別不符。", E);
		}
		catch(FormatException E){
			throw new InvalidOperationException($"寫入 {DeclaringType.FullName}.{CodeName} 失敗：值無法轉換（格式不符）。", E);
		}
	}

	/// 在 Attrs 裏找第一個 TAttr 實例；沒有返回 false。
	public bool TryGetAttr<TAttr>(out TAttr? Attr) where TAttr:Attribute{
		foreach(var A in Attrs){
			if(A is TAttr T){
				Attr = T;
				return true;
			}
		}
		Attr = null;
		return false;
	}
}