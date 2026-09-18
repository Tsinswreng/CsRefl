using Tsinswreng.CsCore;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsRefl.Scripts;

[Doc("""
#Sum[`TestAotWin` 命令的流程實現。]

#Descr[
NativeAOT 開關由模板的 `Directory.Build.props` 統一提供。
]
""")]
internal static partial class TestAotWin{
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		var TestProject = Ctx.RootDir/"proj/Tsinswreng.CsRefl.Test/Tsinswreng.CsRefl.Test.csproj";

		// step 1: 以 Release 配置發布 win-x64 NativeAOT 測試 executable。
		await Exe("dotnet", ["publish", TestProject, "-c", "Release", "-r", "win-x64"], Ct);

		// step 2: 運行 publish 目錄中的原生測試程式；目錄結構固定為 bin/Release/{Tfm}/{Rid}/publish。
		var PublishExe = Ctx.RootDir/"proj/Tsinswreng.CsRefl.Test/bin/Release"/Ctx.Tfm/"win-x64/publish/Tsinswreng.CsRefl.Test.exe";
		await Exe(PublishExe, [], Ct);
	}
}