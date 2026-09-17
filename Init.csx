// 模板初始化腳本：把 Tsinswreng.CsTpl 模板變成新專案的起點。
// 用法：dotnet script Init.csx <NewProjectName>
// 動作：(1) 全域文字替換 TswgCsTpl → <NewProjectName>（.cs/.csproj/.sln/.typ 等）
//       (2) 重命名 proj 下相關目錄與檔案（含 sln）
//       (3) 刪除原 .git 並重新 git init

#r "nuget: Tsinswreng.CsSh, 0.3.0-alpha"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

var CtSource = new System.Threading.CancellationTokenSource();
var Ct = CtSource.Token;

// 參數檢查：只需要新專案名。
if (Args.Count != 1) {
	throw new ArgumentException("需要且只需要一個參數：新專案名。\n用法：dotnet script Init.csx <NewProjectName>");
}
var NewName = Args[0];
if (string.IsNullOrWhiteSpace(NewName) || NewName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
	throw new ArgumentException($"新專案名「{NewName}」非法：不能為空，且不能包含路徑分隔符或非法字符。");
}

// 腳本位於模板根目錄；切換工作目錄，後續相對路徑都以模板根為基準。
var Root = (Pth)Path.GetFullPath(CsxDir());
Cd(Root);
var OldName = "TswgCsTpl";

// step 1: 全域文字替換。
// 排除 .git、bin/、obj/ 與本腳本自身（避免腳本内使用的 OldName 字面量被換掉）。
var ReplacedFileCount = 0;
foreach (var FilePath in LsFile(Root, new LsOptions(Recursive: true))) {
	var Rel = Path.GetRelativePath(Root.Value, FilePath.Value).Replace('\\', '/');
	if (Rel.StartsWith(".git/") || Rel.Contains("/bin/") || Rel.Contains("/obj/"))
		continue;
	if (Path.GetFileName(FilePath.Value) == "Init.csx")
		continue;
	// Write 必須在 Read 的 using 區塊結束後執行：Content 持有檔案句柄，
	// 同一進程在句柄釋放前覆寫同一檔案會觸發 FileShare 衝突。
	string Text;
	await using (var Content = await Read(FilePath, Ct)) {
		Text = await Content.Text(Ct);
	}
	if (Text.Contains(OldName)) {
		await Write(FilePath, Text.Replace(OldName, NewName), Ct);
		ReplacedFileCount++;
	}
}
await Echo($"文字替換完成：{ReplacedFileCount} 個檔案。", Ct);

// step 2: 重命名檔案與目錄。
// 檔案先改名（父目錄此時還是舊名）；目錄按深度由深到淺改名，避免父目錄改名後子路徑失效。
foreach (var FilePath in LsFile(Root, new LsOptions(Recursive: true)).ToList()) {
	var FileName = BaseName(FilePath).Value;
	if (!FileName.Contains(OldName))
		continue;
	await Mv(FilePath, DirName(FilePath) / FileName.Replace(OldName, NewName), Ct);
}
foreach (var DirPath in LsDir(Root, new LsOptions(Recursive: true)).ToList()
	.OrderByDescending(D => D.Value.Count(C => C == '/'))) {
	var DirName_ = BaseName(DirPath).Value;
	if (!DirName_.Contains(OldName))
		continue;
	await Mv(DirPath, DirName(DirPath) / DirName_.Replace(OldName, NewName), Ct);
}
await Echo("檔案與目錄重命名完成。", Ct);

// step 3: 刪除原 .git 並重新初始化。
// 模板自身是 git 倉庫；新專案不應繼承模板的提交歷史。
// git 的對象文件常被標記唯讀（robocopy 複製時保留屬性），Windows 下刪除唯讀文件會失敗，
// 故先遞迴清除 .git 樹的唯讀屬性再刪除。
var OldGit = Root / ".git";
if (await Exists(OldGit, Ct)) {
	foreach (var F in LsFile(OldGit, new LsOptions(Recursive: true)).ToList())
		File.SetAttributes(FullPath(F).Value, FileAttributes.Normal);
	foreach (var D in LsDir(OldGit, new LsOptions(Recursive: true)).ToList())
		File.SetAttributes(FullPath(D).Value, FileAttributes.Normal);
	File.SetAttributes(FullPath(OldGit).Value, FileAttributes.Normal);
	// Rm 等價 rm -rf；若被 IDE 等進程鎖住而失敗，會在此拋出真實錯誤。
	await Rm(OldGit, Ct);
}
await Exe("git", ["init"], Ct);
await Echo($"完成：{NewName} 已就緒，git 倉庫已初始化（尚未產生任何提交）。", Ct);