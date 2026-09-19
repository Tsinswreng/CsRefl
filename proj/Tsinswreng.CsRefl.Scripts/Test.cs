namespace Tsinswreng.CsRefl.Scripts;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[以 `dotnet run` 直接運行 `Tsinswreng.CsRefl.Test` 測試專案。]

#Descr[
例：`dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- {nameof(Test)}`
（不加 `--nologo`：`--` 之後的引數歸本腳本，加了會被當成腳本名而報未知條目）。

本腳本走 JIT，用來快速驗證功能；AOT 路徑見 {nameof(TestAotWin)}。
]
""")]
internal static partial class Test{
	[Doc($"""
#Sum[執行測試；失敗時以非零退出碼結束。]

#Params([[執行上下文，提供模板倉庫根目錄等固定事實], [取消令牌]])

#Descr[
例：`Ctx.{nameof(ISCtx.RootDir)}` 用來拼測試專案路徑，
`Ctx.{nameof(ISCtx.Tfm)}` 用來拼構建輸出目錄；
實現見 `Test.Impl.cs`。
]
""")]
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}