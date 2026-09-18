namespace Tsinswreng.CsRefl.Scripts;

using Tsinswreng.CsCore;

[Doc("""
#Sum[以 `dotnet run` 直接運行 `Tsinswreng.CsRefl.Test` 測試專案。]
""")]
internal static partial class Test{
	[Doc("""
#Sum[執行測試；失敗時以非零退出碼結束。]

#Params([[執行上下文，提供模板倉庫根目錄等固定事實], [取消令牌]])
""")]
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}