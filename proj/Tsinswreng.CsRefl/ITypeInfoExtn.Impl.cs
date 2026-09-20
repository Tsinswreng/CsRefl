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
實測：成員命中但不可讀時返回 false（不拋）；命中且可讀時返回 true 並給出值。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, str Name, obj? O, out obj? V){
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
實測：值型別不符時照常拋（不是返回 false），故「不可寫」與「寫錯型別」兩種情況分得開。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, str Name, obj? O, obj? V){
		// step 1: 同上，O(1) 按名查。
		if(z is null || !z.TryGetMember(Name, out var M)){
			return false;
		}
		// step 2: 寫值的判據由 MemberExtn 收口。
		return MemberExtn.TrySet(M, O, V);
	}
}
