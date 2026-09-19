namespace Tsinswreng.CsRefl.Scripts;

using System.Runtime.CompilerServices;
using Tsinswreng.CsCore;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

[Doc($"""
#Sum[子命令共用的執行上下文。]

#Descr[
dispatcher 統一建立並填入項目固定事實，子命令直接取用，不各自解析路徑。

例：`Test.Main(Ctx, Ct)` 只拿到 {nameof(ISCtx)}，
要路徑就問 {nameof(ISCtx.RootDir)}、要 TFM 就問 {nameof(ISCtx.Tfm)}，
子命令自己不必知道腳本源文件在哪一層。
]
""")]
public interface ISCtx{
	[Doc($"""
#Sum[模板倉庫根目錄，各命令以此定位 `proj/` 下專案。]

#Descr[
例：值是倉庫根（`Tsinswreng.CsRefl` 目錄），
子命令拼測試專案路徑時就是 {nameof(ISCtx.RootDir)} 加 `proj/Tsinswreng.CsRefl.Test`。
]
""")]
	public Pth RootDir{get;set;}

	[Doc($"""
#Sum[目標框架（TFM），與 `proj/Directory.Build.props` 的 `TargetFramework` 同步。]

#Descr[
決定構建與發布輸出目錄名（如 `net10.0`），升級 TFM 時此處需同步更新。

例：`dotnet publish` 的輸出目錄是 `bin/Release/{nameof(ISCtx.Tfm)}/win-x64/`，
故 TFM 一改，測試腳本找產物的路徑也跟著變，兩處必須同步。
]
""")]
	public str Tfm{get;set;}
}

public class SCtx:ISCtx{
	public Pth RootDir{get;set;}

	[Doc($"""
#Sum[默認空串，dispatcher 建立時必定覆蓋，故不允許為 null。]

#Descr[
例：若不給初值，字段初始化就留下 null，
而 {nameof(ISCtx.Tfm)} 在非可空上下文下被當作一定非 null 用；
給空串是為了讓「忘了填」表現為明顯的空值而不是 {nameof(NullReferenceException)}。
]
""")]
	public str Tfm{get;set;} = "";
}

[Doc($"""
#Sum[模板維護腳本的命令列入口。]

#Descr[
第一個引數選擇具體腳本；腳本本身負責完整的一次性流程。

例：`dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- {nameof(Test)}`
跑 JIT 測試；換成 {nameof(TestAotWin)} 就是發布成 win-x64 原生可執行檔再跑一遍。
]
""")]
internal static partial class Program{
	[Doc($"""
#Sum[將命令列入口分派至具名腳本。]

#Params([[命令列引數；首個引數是要執行的腳本名]])

#Descr[
例：`Args` 是 `[nameof(Test)]` 時走 {nameof(Test)}；
空陣列時印用法並以 0 退出；未知名字拋 {nameof(ArgumentException)}，
故 `-- TestAotwin` 這種大小寫拼錯不會被靜默忽略。
]
""")]
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

	[Doc($"""
#Sum[列出可由 `dotnet run -- <entry>` 呼叫的腳本名稱。]

#Descr[
例：輸出兩行，一行是用法、一行是可用條目；
只寫不執行任何構建，故空引數時不會有任何副作用。
]
""")]
	private static void PrintUsage(){
		Console.Error.WriteLine("Usage: dotnet run --project proj/Tsinswreng.CsRefl.Scripts -- <entry>");
		Console.Error.WriteLine("Entries: Test, TestAotWin");
	}

	[Doc($"""
#Sum[讓編譯器提供腳本源文件路徑，故腳本不依賴啟動時的當前目錄。]

#Params([[本腳本源文件路徑；由編譯器以 {nameof(CallerFilePathAttribute)} 自動填入，調用方無需傳入]])

#Descr[
例：`{nameof(OwnPath)}()` 不帶實參調用，
編譯器自動填入本文件的絕對路徑，故從任何工作目錄啟動都能定位倉庫根。
]
""")]
	private static str OwnPath([CallerFilePath] str CallerPath = ""){
		return CallerPath;
	}
}