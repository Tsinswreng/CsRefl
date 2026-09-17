namespace Tsinswreng.CsRefl.Scripts;

/// 以 win-x64 NativeAOT 發布 Tsinswreng.CsRefl.Test 並運行發布物，驗證 AOT 剪裁後測試仍全部通過。
internal static partial class TestAotWin{
	/// 執行：publish -c Release -r win-x64 → 運行 publish 目錄下之測試 exe。
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}