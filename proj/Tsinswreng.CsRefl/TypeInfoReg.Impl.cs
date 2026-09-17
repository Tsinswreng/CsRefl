namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// TypeInfoReg 的函數實現。
public partial class TypeInfoReg{
	/// 取已註冊型別的元資料；未註冊返回 false。
	public bool TryGetInfo(
		// DAM 註解與接口一致（註冊表本身不依賴成員元數據）。
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	){
		ArgumentNullException.ThrowIfNull(Type);
		lock(_sync){
			return _map.TryGetValue(Type, out Info);
		}
	}

	/// 已註冊型別快照（在鎖內拷貝，返回一次性列表）。
	private IReadOnlyCollection<Type>? SnapshotTypes(){
		lock(_sync){
			return _map.Keys.ToList();
		}
	}

	/// 登記一個型別的元資料；重複登記拋 InvalidOperationException
	/// （防止無意覆蓋；確要替換先 Remove 再 Add）。
	public void Add(Type Type, ITypeInfo Info){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Info);
		lock(_sync){
			if(_map.ContainsKey(Type)){
				throw new InvalidOperationException($"型別 {Type.FullName} 已註冊，不能用 Add 覆蓋；請先 Remove 再 Add。");
			}
			_map.Add(Type, Info);
		}
	}

	/// 移除一個型別；原本不存在返回 false。
	public bool Remove(Type Type){
		ArgumentNullException.ThrowIfNull(Type);
		lock(_sync){
			return _map.Remove(Type);
		}
	}
}