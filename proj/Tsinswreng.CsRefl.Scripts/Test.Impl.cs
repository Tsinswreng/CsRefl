using Tsinswreng.CsCore;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsRefl.Scripts;

[Doc("""
#Sum[`Test` 命令的流程實現。]
""")]
internal static partial class Test{
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		// dotnet run 會自動還原並建置；輸出必須轉送終端，否則看不到測試結果。
		await Exe("dotnet", ["run", "--project", Ctx.RootDir/"proj/Tsinswreng.CsRefl.Test/Tsinswreng.CsRefl.Test.csproj"], Ct);
	}
}