namespace Tsinswreng.CsRefl.Scripts;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[以 win-x64 NativeAOT 發布 `Tsinswreng.CsRefl.Test` 並運行發布物，驗證 AOT 剪枝後測試仍全部通過。]

#Descr[
例：`dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- {nameof(TestAotWin)}`
先在 `bin/Release/{nameof(ISCtx.Tfm)}/win-x64/publish/` 產出原生 exe，
再跑那份 exe；測試結果與 JIT 路徑一致才算過。

為甚麼要單獨一條：JIT 下的反射與表達式樹都可用，
AOT 下表達式樹不能動態編譯、反射成員可能被剪掉，
故此命令是「兼容 AOT」這個承諾的唯一實證。
]
""")]
internal static partial class TestAotWin{
	[Doc($"""
#Sum[執行：`publish -c Release -r win-x64`，然後運行 publish 目錄下之測試 exe。]

#Params([[執行上下文，提供模板倉庫根目錄、TFM 等固定事實], [取消令牌]])

#Descr[
例：`Ctx.{nameof(ISCtx.Tfm)}` 用來拼發布輸出目錄（如 `net10.0`），
故 TFM 升級時本腳本不必改；實現見 `TestAotWin.Impl.cs`。
]
""")]
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}