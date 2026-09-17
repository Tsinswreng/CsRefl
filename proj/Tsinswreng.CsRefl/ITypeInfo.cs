namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// 一個型別的元資料：分類、成員表、實例工廠、集合/字典的鍵值型別。
/// 兩套來源（反射、JsonTypeInfo）各自實現本接口，對外語義一致。
public interface ITypeInfo{
	/// 本元資料對應的型別。
	Type Type{get;}
	/// 型別分類：標量 / 物件 / 集合 / 字典。
	ETypeKind Kind{get;}
	/// 本型別能否建立無參實例（存在無參構造函數，或值型別）。
	bool CanMkInst{get;}
	/// 建立一個無參實例。CanMkInst 為 false 時拋 NotSupportedException。
	obj? MkInst();
	/// 集合的元素型別；非集合為 null。相當於 JsonTypeInfo.ElementType。
	Type? ElemType{get;}
	/// 字典的鍵型別；非字典為 null。相當於 JsonTypeInfo.KeyType。
	Type? KeyType{get;}
	/// 全部成員，順序 = 契約序：基類在前、同類內按來源的宣告序
	/// （反射源：元數據表序；Json 源：Properties 既有序）。見 TypeInfoSorter。
	IReadOnlyList<IMemberInfo> Members{get;}
	/// 按鍵（CodeName）查成員；未知返回 false。
	bool TryGetMember(str CodeName, [NotNullWhen(true)] out IMemberInfo? M);
	/// 按鍵（CodeName）取成員；未知拋 KeyNotFoundException，訊息含可用鍵清單。
	IMemberInfo GetMember(str CodeName);
	/// 可讀成員名清單，順序同 Members。
	IReadOnlyCollection<str> ReadableNames{get;}
	/// 可寫成員名清單，順序同 Members。
	IReadOnlyCollection<str> WritableNames{get;}
}