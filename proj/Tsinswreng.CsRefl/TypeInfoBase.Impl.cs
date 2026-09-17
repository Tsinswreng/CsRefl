namespace Tsinswreng.CsRefl;

/// TypeInfoBase 的函數實現。
public abstract partial class TypeInfoBase{
	/// 惰性建立按名索引；Members 在建構後不可變，故緩存安全。
	private void EnsureByName(){
		if(_byName is not null){
			return;
		}
		var Dict = new Dictionary<str, IMemberInfo>(Members.Count, StringComparer.Ordinal);
		foreach(var M in Members){
			Dict[M.CodeName] = M;
		}
		_byName = Dict;
	}

	/// 按鍵查成員；未知返回 false。
	public bool TryGetMember(str CodeName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IMemberInfo? M){
		EnsureByName();
		M = null;
		return _byName!.TryGetValue(CodeName, out M);
	}

	/// 按鍵取成員；未知拋 KeyNotFoundException，訊息含可用鍵清單。
	public IMemberInfo GetMember(str CodeName){
		EnsureByName();
		if(_byName!.TryGetValue(CodeName, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {Type.FullName} 沒有成員 {CodeName}。可用成員：{string.Join(", ", Members.Select(x => x.CodeName))}"
		);
	}
}