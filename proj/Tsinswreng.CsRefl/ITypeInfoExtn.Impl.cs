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
兩步：按名查成員（走 {nameof(ITypeInfo.TryGetMember)} 的惰性索引，O(1)）→ 讀值判據交給 {nameof(MemberExtn)}。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, obj? O, str Name, out obj? V){
		V = default;
		// step 1: 按名查成員；實現走惰性索引，是 O(1)（見 ITypeInfo.TryGetMember）。
		if(z is null || !z.TryGetMember(Name, out var M)){
			return false;
		}
		// step 2: 讀值的判據（成員自身能力、實例型別、實例可空）由 MemberExtn 收口。
		return MemberExtn.TryGet(M, O, out V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
同上兩步；寫值判據交給 {nameof(MemberExtn)}，值型別不符照常拋、不吞成 false。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, obj? O, str Name, obj? V){
		// step 1: 同上，O(1) 按名查。
		if(z is null || !z.TryGetMember(Name, out var M)){
			return false;
		}
		// step 2: 寫值的判據由 MemberExtn 收口。
		return MemberExtn.TrySet(M, O, V);
	}
}
