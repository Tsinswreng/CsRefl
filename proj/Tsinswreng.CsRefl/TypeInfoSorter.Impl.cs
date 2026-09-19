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

#Descr[
為甚麼要按名去重（防禦性）：
成員名就是字典鍵與 SQL 列名，同名兩份會讓
{nameof(ITypeInfo.Members)}、{nameof(ITypeInfo.ReadableNames)}、按名索引三處口徑分裂
（索引靜默取一份、清單裏卻有兩項）。

實測事實：.NET 10 的 {nameof(Type)}.{nameof(Type.GetProperties)} 本身就不返回被 `new` 遮蔽的基類屬性，
官方的 {nameof(System.Text.Json.Serialization.Metadata.JsonTypeInfo.Properties)} 也不含重複名——
也就是說兩套現有來源目前都不產生重複項。
這裡仍按契約去重：
門面對外承諾「成員名唯一」，不依賴來源剛好守規矩。

去重規則：同名的保留「離實例最近」的那份宣告（子類優先），
並佔用被遮蔽成員原先的位置——
如此非遮蔽成員的相對序完全不變。

實測：`PoUser` 這條鏈的成員表依次是
`Id`、`Name`、`Age`、`Email`、`Married`、`Tags`、`Extra`、`Secret`、`Level`、`Token`、`Note`；
另一條鏈上子類用 `new` 遮蔽了基類的 `Id`，輸出是 `Name`、`Id`、`Age` 三項
（`Id` 只出現一次、仍在第 2 位、按名查到的那份宣告型別是子類）。
]
""")]
	public static partial IReadOnlyList<IMemberInfo> SortEtDedup(Type Root, IReadOnlyList<IMemberInfo> Members){
		ArgumentNullException.ThrowIfNull(Root);
		ArgumentNullException.ThrowIfNull(Members);

		// step 1: 排序。
		// 契約序 = 基類在前：DepthOf 是「自 Root 沿 BaseType 下探的層數」，
		// 基類的值更大，故用降序。LINQ OrderBy 穩定，同深度保持來源給的相對序。
		// DeclaringType 為 null（畸形元資料，官方 JsonPropertyInfo.DeclaringType 本來就可空）
		// 時按 0 處理，即「當作 Root 自己宣告」，排在最前。
		var Sorted = Members.OrderByDescending(M => M.DeclaringType is null ? 0 : DepthOf(Root, M.DeclaringType)).ToList();

		// step 2: 挑勝出者。
		// 正序掃一遍、首見者勝出即得「離實例最近」的那份宣告：降序排列下派生類成員在前。
		var ByName = new Dictionary<str, IMemberInfo>(Sorted.Count, StringComparer.Ordinal);
		foreach(var M in Sorted){
			ByName.TryAdd(M.Name, M);
		}

		// step 3: 按「首次出現」的位置輸出。
		// 輸出位置 = 該名在契約序裏「首次出現」的位置（不是勝出者自身的位置）：
		// 遮蔽時勝出的是派生類那份、排在基類之後，但契約要求它佔基類那份的位置，
		// 否則成員序會從 Id,Name,Age 變成 Name,Id,Age。
		var Placed = new HashSet<str>(ByName.Count, StringComparer.Ordinal);
		var R = new List<IMemberInfo>(ByName.Count);
		foreach(var M in Sorted){
			if(Placed.Add(M.Name)){
				R.Add(ByName[M.Name]);
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