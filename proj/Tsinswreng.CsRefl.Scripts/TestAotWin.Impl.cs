using Tsinswreng.CsCore;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsRefl.Scripts;

[Doc($"""
#Sum[{nameof(TestAotWin)} 命令的流程實現。]

#Descr[
NativeAOT 開關由模板的 `Directory.Build.props` 統一提供。

例：本體是「發布一次、跑一次」兩步，
故任何 AOT 失效（剪枝剪掉成員、表達式樹不能編譯）都會在第二步以非零退出碼暴露。
]
""")]
internal static partial class TestAotWin{
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		var TestProject = Ctx.RootDir/"proj/Tsinswreng.CsRefl.Test/Tsinswreng.CsRefl.Test.csproj";

		// step 1: 以 Release 配置發布 win-x64 NativeAOT 測試 executable。
		await Exe("dotnet", ["publish", TestProject, "-c", "Release", "-r", "win-x64"], Ct);

		// step 2: 運行 publish 目錄中的原生測試程式；目錄結構固定為 bin/Release/{Tfm}/{Rid}/publish。
		// 目錄名用 Ctx.Tfm 拼（而非硬編碼 net10.0），升級 TFM 時不必改本腳本。
		var PublishExe = Ctx.RootDir/"proj/Tsinswreng.CsRefl.Test/bin/Release"/Ctx.Tfm/"win-x64/publish/Tsinswreng.CsRefl.Test.exe";
		await Exe(PublishExe, [], Ct);
	}
}