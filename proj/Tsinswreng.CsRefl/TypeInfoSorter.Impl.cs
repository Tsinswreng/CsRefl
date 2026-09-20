namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(TypeInfoSorter)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `TypeInfoSorter.cs`。
]
""")]
internal static partial class TypeInfoSorter{
	[Doc($"""
#Sum[規整成員表：排序 + 按名去重（保證每個成員名只出現一次）。]

#Params([[Root, 成員表所屬的型別，即實例的型別], [Members, 待規整的成員表]])

#Rtn[規整後的只讀成員表]

""")]
	public static partial IReadOnlyList<obj?> SortEtDedup(Type Root, IReadOnlyList<obj?> Members){
		ArgumentNullException.ThrowIfNull(Root);
		ArgumentNullException.ThrowIfNull(Members);

		// step 1: 排序。
		// 契約序 = 基類在前：DepthOf 是「自 Root 沿 BaseType 下探的層數」，
		// 基類的值更大，故用降序。LINQ OrderBy 穩定，同深度保持來源給的相對序。
		// DeclaringType 為 null（畸形元資料，官方 JsonPropertyInfo.DeclaringType 本來就可空）
		// 時按 0 處理，即「當作 Root 自己宣告」，排在最前。
		var Sorted = Members
			.OrderByDescending(M => Member.DeclaringType(M) is null ? 0 : DepthOf(Root, Member.DeclaringType(M)!))
			.ToList();

		// step 2: 挑勝出者。
		// 正序掃一遍、首見者勝出即得「離實例最近」的那份宣告：降序排列下派生類成員在前。
		// 名字由 Member 從官方成員物件取，兩側同一條口徑。
		var ByName = new Dictionary<str, obj?>(Sorted.Count, StringComparer.Ordinal);
		foreach(var M in Sorted){
			ByName.TryAdd(Member.Name(M), M);
		}

		// step 3: 按「首次出現」的位置輸出。
		// 輸出位置 = 該名在契約序裏「首次出現」的位置（不是勝出者自身的位置）：
		// 遮蔽時勝出的是派生類那份、排在基類之後，但契約要求它佔基類那份的位置，
		// 否則成員序會從 Id,Name,Age 變成 Name,Id,Age。
		var Placed = new HashSet<str>(ByName.Count, StringComparer.Ordinal);
		var R = new List<obj?>(ByName.Count);
		foreach(var M in Sorted){
			var N = Member.Name(M);
			if(Placed.Add(N)){
				R.Add(ByName[N]);
			}
		}
		// step 4: 包成只讀：成員表對外不可變，按名索引緩存纔安全。
		return R.AsReadOnly();
	}

	private static partial int DepthOf(Type Root, Type Declaring){
		var D = 0;
		var T = Declaring;
		while(T is not null && T != Root){
			T = T.BaseType;
			D++;
		}
		return D;
	}
}

