namespace Tsinswreng.CsRefl.Scripts;

using System.Runtime.CompilerServices;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

/// 子命令共用的執行上下文。
/// dispatcher 統一建立並填入項目固定事實，子命令直接取用，不各自解析路徑。
public interface ISCtx{
	/// 模板倉庫根目錄，各命令以此定位 proj/ 下專案。
	public Pth RootDir{get;set;}
	/// 目標框架（TFM），與 proj/Directory.Build.props 的 TargetFramework 同步；
	/// 決定構建/發布輸出目錄名（如 net10.0），升級 TFM 時此處需同步更新。
	public str Tfm{get;set;}
}

public class SCtx:ISCtx{
	public Pth RootDir{get;set;}
	/// 默認空串，dispatcher 建立時必定覆蓋，故不允許為 null。
	public str Tfm{get;set;} = "";
}

/// 模板維護腳本的命令列入口。
/// 第一個引數選擇具體腳本；腳本本身負責完整的一次性流程。
internal static partial class Program{
	/// 將命令列入口分派至具名腳本。
	internal static async Task Main(str[] Args){
		var Ct = default(CT);
		// Program.cs 位於 <根>/proj/Tsinswreng.CsRefl.Scripts；上推兩級得到倉庫根。
		var Root = FullPath(DirName(OwnPath())/"../..");

		// step 1: 建立統一的子命令上下文，所有子命令都從中取得 RootDir、Tfm。
		ISCtx Ctx = new SCtx {
			RootDir = Root,
			// 與 proj/Directory.Build.props 的 TargetFramework 同步；升級 TFM 時兩處一起改。
			Tfm = "net10.0",
		};

		if(Args.Length == 0){
			PrintUsage();
			return;
		}

		// step 2: 依首個引數分派；新命令在此與 PrintUsage 各加一行。
		switch(Args[0]){
			case nameof(Test):
				await Test.Main(Ctx, Ct);
				break;
			case nameof(TestAotWin):
				await TestAotWin.Main(Ctx, Ct);
				break;
			default:
				throw new ArgumentException($"Unknown Tsinswreng.CsRefl script: {Args[0]}.", nameof(Args));
		}
	}

	/// 列出可由 dotnet run -- <entry> 呼叫的腳本名稱。
	private static void PrintUsage(){
		Console.Error.WriteLine("Usage: dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- <entry>");
		Console.Error.WriteLine("Entries: Test, TestAotWin");
	}

	/// 讓編譯器提供腳本源文件路徑，故腳本不依賴啟動時的當前目錄。
	private static str OwnPath([CallerFilePath] str CallerPath = ""){
		return CallerPath;
	}
}