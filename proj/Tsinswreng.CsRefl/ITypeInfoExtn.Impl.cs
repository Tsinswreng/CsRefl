namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ITypeInfoExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `ITypeInfoExtn.cs`。
]
""")]
public static partial class ITypeInfoExtn{
	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
兩步：按名查成員（走 {nameof(ITypeInfo.TryGetMember)} 的惰性索引，O(1)）→ 讀值判據交給 {nameof(IMemberInfo)}。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, obj? O, str Name, out obj? V){
		V = default;
		// step 1: 按名查成員；實現走惰性索引，是 O(1)（見 ITypeInfo.TryGetMember）。
		if(z is null || !z.TryGetMember(Name, out var M)){
			return false;
		}
		// step 2: 讀值的判據（成員自身能力、實例型別、實例可空）由 Member 收口。
		return M.TryGet(O, out V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
泛型版與非泛型版同一條口徑：受體（型別元資料）已經帶着型別，故這裡沒有「型別從哪來」的問題；
差別只是實例以它的靜態型別傳入（裝箱不改變任何讀寫判據）。
]
""")]
	public static partial bool TryGet<T>(this ITypeInfo z, T O, str Name, out obj? V){
		// 用顯式靜態調用轉非泛型版：寫成 z.TryGet(O, ...) 的話，
		// C# 會挑回本泛型版自己（T 對 T 是恆等轉換，比轉成 obj 更貼）→ 無限遞歸。
		return ITypeInfoExtn.TryGet(z, (obj?)O, Name, out V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
同上兩步；寫值判據交給 {nameof(IMemberInfo)}，值型別不符照常拋、不吞成 false。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, obj? O, str Name, obj? V){
		// step 1: 同上，O(1) 按名查。
		if(z is null || !z.TryGetMember(Name, out var M)){
			return false;
		}
		// step 2: 寫值的判據由 Member 收口。
		return M.TrySet(O, V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
泛型版與非泛型版同一條口徑：判據全部落在成員本體上，值型別不符照樣拋、不吞成 false。
]
""")]
	public static partial bool TrySet<T>(this ITypeInfo z, T O, str Name, obj? V){
		// 同 TryGet<T>：用顯式靜態調用，否則會挑回本泛型版自己。
		return ITypeInfoExtn.TrySet(z, (obj?)O, Name, V);
	}


}






