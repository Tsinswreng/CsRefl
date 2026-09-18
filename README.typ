// 自動標題庫
#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading
#let P(C) = {C}

#H[Tsinswreng.CsRefl][
	#H[這是什麼][
		#P[
			CsRefl 是 Ngan 系的統一反射門面，
			一模一樣的接口背後是兩套實現：
		]

		- #[兼容 AOT 的反射（ReflTypeInfoSrc），
			任何型別都能查，
			前提是成員元數據被保留（見下）。]
		- #[JsonTypeInfo 源生成（JsonTypeInfoSrc），
			只認 [JsonSerializable] 註冊過的型別，
			讀寫是 STJ 的源生成委託，
			是 AOT 下的主路徑。]

		#P[
			兩者可以合成（MergedTypeInfoSrc），
			按優先級逐一查詢：
			Json 優先、手寫註冊表（TypeInfoReg）其次、反射最後兜底。
		]
	]

	#H[快速開始][
		#P[
			在 DI 裏組合來源，
			之後對外只碰 ITypeInfoSrc 的擴展方法：
		]

		```csharp
		SvcColct.AddSingleton<JsonSerializerContext>(AppJsonCtx.Default);
		SvcColct.AddSingleton<JsonTypeInfoSrc>();
		SvcColct.AddSingleton<ReflTypeInfoSrc>();
		SvcColct.AddSingleton<TypeInfoReg>();
		SvcColct.AddSingleton(sp => new MergedTypeInfoSrc(
			sp.GetRequiredService<JsonTypeInfoSrc>(),
			sp.GetRequiredService<TypeInfoReg>(),
			sp.GetRequiredService<ReflTypeInfoSrc>()
		));
		```

		#P[
			用法（ITypeInfoSrcExtn 擴展方法）：
		]

		- `GetMember(Type, Name)` / `TryGet(...)` / `TrySet(...)`：按名讀寫。
		- `ToInstDict(O)`：把物件變成淺字典視圖（讀寫直接作用回物件）。
		- `AssignFromDict(O, Dict)`：把字典寫回物件（未知鍵拋異常，
			只讀成員跳過）。
	]

	#H[運行測試][
		#P[
			測試用 CsTreeTest，從 NuGet 引用：
		]

		```sh
		dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- Test
		dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- TestAotWin
		```

		#P[
			Test 是 JIT 快速驗證，
			TestAotWin 以 win-x64 NativeAOT 發布測試項目後運行發布物，
			驗證 AOT 下兩套實現都可用。
		]
	]

	#H[AOT 前提（重要）][
		#P[
			反射來源在 AOT 下可用的前提是成員元數據被保留。
			最省事的做法是讓型別掛上 [JsonSerializable]——
			STJ 源生成會保住成員元數據，
			反射來源就能查到 （PropertyInfo.GetValue/SetValue 可用）。
			無參實例：JIT 用表達式樹一次編譯成委託，
			NativeAOT 退回 Activator.CreateInstance。
		]

		#P[
			已驗證：win-x64 NativeAOT 發布零警告、產物為自包含原生 exe，
			測試 64/64 全過（兩套來源都在原生下可用）。
		]
	]

	#H[淺字典視圖的口徑][
		#P[
			IInstDict（InstDict）有兩條口徑，
			職責不同、不可混為一談：
		]

		- #[出現口徑（`Keys` / `Count` / `Values` / 枚舉）：
			可讀且可寫的成員，
			順序 = 成員序。]
		- #[訪問口徑（索引器 / `TryGetValue` / `ContainsKey`）：
			讀寫各按成員自身能力放行——
			只讀成員讀得到、寫不進；
			只寫成員寫得進、讀不到。]

		#P[
			兩者的差別是故意的：
			字典視圖要能當普通字典改值，
			又不該因為某成員只讀就把它的值藏起來。
			`Keys` 是活視圖（與視圖同一份鍵集合），
			但不提供改形狀的入口；
			`Add` / `Remove` / `Clear` 恆拋 NotSupportedException。
		]
	]

	#H[與官方 API 的關係（設計原則）][
		#P[
			本包是「一套接口、兩套實現」：
			對外只暴露同一組接口，
			底下分別由 AOT 反射與 JsonTypeInfo 源生成填滿。
			接口設計的規則是**官方已有的概念一律沿用官方的名字與型別**，
			只有官方確實沒有的才自研。
		]

		#P[
			官方到底有甚麼（2026-09-17 逐條實測，含 win-x64 NativeAOT）：
		]

		- #[官方**沒有**共同的成員基類：反射側是 `MemberInfo`
			（`PropertyInfo`／`FieldInfo` 的基類），
			JsonTypeInfo 側是 `JsonPropertyInfo`，
			而 `JsonPropertyInfo` 是獨立抽象類、**不繼承** `MemberInfo`。]
		- #[官方兩側**共有**的是 `ICustomAttributeProvider`：
			`MemberInfo` 實現它，
			`JsonPropertyInfo` 也提供 `AttributeProvider` 給它。]
		- #[所以門面開兩個對稱的官方出口：
			`IMemberInfo.Member`（反射成員本體，Json 源為 null）
			與 `IMemberInfo.Json`（官方 `JsonPropertyInfo`，反射源為 null）；
			型別層同理：`ITypeInfo.Json` 給官方 `JsonTypeInfo` 本體。]

		#P[
			逐個對上的官方名字與型別：
		]

		- #[成員名 `Name`（官方 `MemberInfo.Name`／`JsonPropertyInfo.Name`）。]
		- #[成員型別 `PropertyType`（官方 `JsonPropertyInfo.PropertyType`）。]
		- #[宣告型別 `DeclaringType`、成員種類 `MemberType`（官方 `MemberTypes`）。]
		- #[讀寫 `Get`／`Set`：型別與名字照官方
			`JsonPropertyInfo.Get`／`Set`
			（`Func<object,object?>`／`Action<object,object?>`）。]
		- #[型別分類 `Kind` 直接用官方 `JsonTypeInfoKind`
			（標量對應官方的 `None`）。]
		- #[`ElementType`／`KeyType` 與官方 `JsonTypeInfo` 同名。]
		- #[無參實例工廠 `CreateObject`，
			型別與語義照官方 `JsonTypeInfo.CreateObject`（`Func<object>?`）；
			「能不能建」就是它是否為 null。]
		- #[特性 `AttributeProvider`（官方 `ICustomAttributeProvider`），
			取值用官方風格的 `IMemberInfo.GetCustomAttribute<T>()`
			（官方只把該擴展掛在具體型別上，
			兩側共有的接口上沒有，故本包補一個同語義的）。]

		#P[
			官方沒有的只有一件：
			**以字符串為鍵**。
			官方兩側都沒有「按名取成員／按名讀寫」的抽象
			（`MemberInfo` 無名索引器，
			`JsonTypeInfo.Properties` 只是 `IList`），
			故自研的只有按名查詢／讀寫（`TryGetMember`／`GetMember`／
			`TryGet`／`TrySet`）、成員表 `Members`、
			名清單 `ReadableNames`／`WritableNames`，
			以及淺字典視圖 `IInstDict`。
		]
	]

	#H[設計記錄][
		- #[成員序 = 契約序：
			基類在前、
			同類內屬性段在字段段前、
			段內保持來源宣告序；
			兩套來源統一由 TypeInfoSorter.SortEtDedup 規整。]
		- #[成員名唯一：
			同名遮蔽（new）時保留離實例最近的宣告，
			並佔用被遮蔽成員的位置。
			實測 .NET 10 的 Type.GetProperties 本身就不返回被遮蔽的基類屬性、
			STJ 的 Properties 也不含重複名，
			故這層去重是防禦性的契約保證，
			不是修一個必然出現的 bug（見 TestShadowing 用例）。]
		- #[公開面判據：
			屬性的可讀/可寫按「有沒有公開訪問器」判定
			（`PropertyInfo.GetGetMethod()`/`GetSetMethod()`），
			而不是 `PropertyInfo.CanRead`/`CanWrite`——
			屬性帶私有 get 時 `CanRead` 仍為 true，
			用它會把只寫屬性誤判成可讀。
			兩套來源統一為「官方委託非 null 即可讀/可寫」這一條判據。]
		- #[Json 來源的能力差別（實測，非推測）：
			成員種類只能報 `Property`——
			官方 `JsonPropertyInfo` 不暴露 `IsProperty`，
			分不出源生成收進來的字段（`[JsonInclude]` 的字段在實測中連
			`AttributeProvider` 都給出 `RtFieldInfo`）。
			特性則**可以**取到：源生成的 `AttributeProvider` 給出
			`RuntimePropertyInfo`／`RtFieldInfo`，
			型別上標的特性在 JIT 與 win-x64 NativeAOT 下都取得回來
			（早期「源生成下特性統一為空」的判斷已被實測推翻）。]
		- #[非公開成員、靜態成員、索引器一律不進門面。]
		- #[成員名 == JSON 名的前提是命名策略為 null 且無 `[JsonPropertyName]`，
			調用方負責保證。]
		- #[緩存：
			JsonTypeInfoSrc 命中與未命中都進緩存
			（未註冊型別記入負面緩存，
			避免批量場景反復重走 resolver 鏈）；
			MergedTypeInfoSrc 的 params 數組做防禦拷貝，
			來源優先級不受調用方事後改動影響。]
	]
]