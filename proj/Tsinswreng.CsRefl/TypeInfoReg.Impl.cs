namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// TypeInfoReg 的函數實現。
/// 只放函數實現：字段與訪問器在 TypeInfoReg.cs。
/// 參數特性（DAM/NotNullWhen）只寫在聲明側，partial 合併時兩邊都標會報 CS0579。
public partial class TypeInfoReg{
	/// 取已註冊型別的元資料；未註冊返回 false。
	public partial bool TryGetInfo(Type Type, out ITypeInfo? Info){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryGetValue(Type, out Info);
	}

	/// 已註冊型別快照（取一份鍵的拷貝，返回一次性列表）。
	private IReadOnlyCollection<Type>? SnapshotTypes(){
		return _map.Keys.ToList();
	}

	/// 登記一個型別的元資料；重複登記拋 InvalidOperationException
	/// （防止無意覆蓋；確要替換先 Remove 再 Add）。
	public partial void Add(Type Type, ITypeInfo Info){
		ArgumentNullException.ThrowIfNull(Type);
		ArgumentNullException.ThrowIfNull(Info);
		if(!_map.TryAdd(Type, Info)){
			throw new InvalidOperationException($"型別 {Type.FullName} 已註冊，不能用 Add 覆蓋；請先 Remove 再 Add。");
		}
	}

	/// 移除一個型別；原本不存在返回 false。
	public partial bool Remove(Type Type){
		ArgumentNullException.ThrowIfNull(Type);
		return _map.TryRemove(Type, out _);
	}
}