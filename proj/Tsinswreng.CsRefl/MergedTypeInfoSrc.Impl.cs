namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// MergedTypeInfoSrc 的函數實現。
public partial class MergedTypeInfoSrc{
	/// 按優先級順序給出來源。
	public MergedTypeInfoSrc(params ITypeInfoSrc[] Sources){
		ArgumentNullException.ThrowIfNull(Sources);
		if(Sources.Length < 1){
			throw new ArgumentException("至少要一個來源。", nameof(Sources));
		}
		_sources = Sources;
	}

	/// 第一個答「已知」的來源勝出；全部答「未知」返回 false。
	public bool TryGetInfo(
		// DAM 註解與接口一致（本類只轉發，不直接接觸成員元數據）。
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	){
		ArgumentNullException.ThrowIfNull(Type);
		foreach(var S in _sources){
			if(S.TryGetInfo(Type, out Info)){
				return true;
			}
		}
		Info = null;
		return false;
	}

	/// 全部來源都支持列舉才返回並集，否則返回 null。
	private IReadOnlyCollection<Type>? SnapshotTypes(){
		var R = new List<Type>();
		foreach(var S in _sources){
			var T = S.RegisteredTypes;
			if(T is null){
				return null;
			}
			foreach(var Item in T){
				if(!R.Contains(Item)){
					R.Add(Item);
				}
			}
		}
		return R;
	}
}