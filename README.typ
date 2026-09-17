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

		- `GetMember(Type, CodeName)` / `TryGet(...)` / `TrySet(...)`：按名讀寫。
		- `ToInstDict(O)`：把物件變成淺字典視圖（鍵 = 可讀可寫成員，
			讀寫直接作用回物件）。
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
	]

	#H[設計記錄][
		- #[成員序 = 契約序：
			基類在前、
			同類內屬性段在字段段前、
			段內保持來源宣告序；
			兩套來源統一排序（TypeInfoSorter）。]
		- #[Json 來源的能力差別：
			成員 Kind 恆為 Property（無 IsProperty 可查）、
			Attrs 恒空、TryGetAttr 恒 false（AttributeProvider 走反射路徑，
			AOT 不可靠，統一不支持）。]
		- #[非公開成員、靜態成員、索引器一律不進門面。]
		- #[JsonName == CodeName 的前提是命名策略為 null 且無 [JsonPropertyName]，
			調用方負責保證。]
	]
]