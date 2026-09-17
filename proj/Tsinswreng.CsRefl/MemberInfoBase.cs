namespace Tsinswreng.CsRefl;

/// 成員元資料的公共基類：把「讀寫動作」與「元數據事實」分開。
/// 兩套來源各自提供事實（CodeName/Kind/型別/可否讀寫/順序/特性）和兩個讀寫委託，
/// TryGet/TrySet 的型別與可用性前置檢查在這裡統一實現，避免兩套來源重複。
///
/// 讀寫委託的語義：GetFn 為 null 即不可讀，SetFn 為 null 即不可寫；
/// 委託只負責取值/賦值，不做任何檢查。
/// 建構子見 MemberInfoBase.Impl.cs（本類只有這一個受保護建構子）。
public abstract partial class MemberInfoBase:IMemberInfo{
	/// 讀值委託；null 表示不可讀。參數/返回與 System.Text.Json 的 Get 委託形狀一致。
	private readonly Func<obj, obj?>? _getFn;
	/// 寫值委託；null 表示不可寫。
	private readonly Action<obj, obj?>? _setFn;

	public abstract str CodeName{get;}
	public abstract str? JsonName{get;}
	public abstract EMemberKind Kind{get;}
	public abstract Type DeclaredType{get;}
	public abstract Type DeclaringType{get;}
	public abstract bool CanRead{get;}
	public abstract bool CanWrite{get;}
	public abstract i32 Order{get;}
	public abstract IReadOnlyList<Attribute> Attrs{get;}

	// TryGet / TrySet / TryGetAttr 的實現見 MemberInfoBase.Impl.cs。
}