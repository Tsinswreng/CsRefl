namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/// 可寫的型別元資料註冊表：手動塞 ITypeInfo、支持列舉，供「反射/JSON 都覆蓋不到」
/// 的場合（如 CsSql 內部的 SchemaHistory，手寫 5 個成員的元資料即可）。
/// 線程安全：讀寫都走 ConcurrentDictionary；初始化期填完之後當只讀用也安全。
/// TryGetInfo / Add / Remove 的實現見 TypeInfoReg.Impl.cs。
public partial class TypeInfoReg:ITypeInfoReg{
	/// 型別 → 元資料表。
	private readonly ConcurrentDictionary<Type, ITypeInfo> _map = new();

	/// 列舉已註冊型別（快照）。
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return SnapshotTypes();
		}
	}

	/// 取已註冊型別的元資料；未註冊返回 false。
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
	/// 登記一個型別的元資料；重複登記拋 InvalidOperationException
	/// （防止無意覆蓋；確要替換先 Remove 再 Add）。
	public partial void Add(Type Type, ITypeInfo Info);
	/// 移除一個型別；原本不存在返回 false。
	public partial bool Remove(Type Type);
}