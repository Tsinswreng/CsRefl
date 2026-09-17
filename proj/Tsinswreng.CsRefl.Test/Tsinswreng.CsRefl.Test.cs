using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Tsinswreng.CsRefl;
using Tsinswreng.CsRefl.Test.Domains.Models;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsRefl.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;
	public static async Task Main(string[] args){
		// 兩套實現 + 手寫註冊表 + 合成來源。
		// 合成順序即優先級：Json（委託、AOT 主徑）→ Reg（手工精確，必須在
		// Refl 之前，否則會被全能反射覆蓋）→ Refl（最後兜底）。
		SvcColct.AddSingleton<JsonSerializerContext>(TestJsonCtx.Default);
		SvcColct.AddSingleton<JsonTypeInfoSrc>();
		SvcColct.AddSingleton<ReflTypeInfoSrc>();
		SvcColct.AddSingleton<TypeInfoReg>();
		SvcColct.AddSingleton(sp => new MergedTypeInfoSrc(
			sp.GetRequiredService<JsonTypeInfoSrc>(),
			sp.GetRequiredService<TypeInfoReg>(),
			sp.GetRequiredService<ReflTypeInfoSrc>()
		));

		var mgr = CsReflTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct, sc => sc.BuildServiceProvider());

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}