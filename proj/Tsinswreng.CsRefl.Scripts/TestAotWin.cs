namespace Tsinswreng.CsRefl.Scripts;

using Tsinswreng.CsCore;

[Doc("""
#Sum[以 win-x64 NativeAOT 發布 `Tsinswreng.CsRefl.Test` 並運行發布物，驗證 AOT 剪枝後測試仍全部通過。]
""")]
internal static partial class TestAotWin{
	[Doc("""
#Sum[執行：`publish -c Release -r win-x64` → 運行 publish 目錄下之測試 exe。]

#Params([[執行上下文，提供模板倉庫根目錄、TFM 等固定事實], [取消令牌]])
""")]
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}