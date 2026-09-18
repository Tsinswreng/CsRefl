namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc("""
#Sum[可寫的型別元資料註冊表：在 `ITypeInfoSrc` 之上補上手動增刪。]

#Descr[
與舊 Srefl 的「可寫字典」不同：
這裡註冊的內容是 `ITypeInfo`（元資料對象），
而不是一張可以被任意替換的字典；
合併與優先級交給 `MergedTypeInfoSrc`。
]
""")]
public interface ITypeInfoReg:ITypeInfoSrc{
	[Doc("""
#Sum[登記一個型別的元資料。]

#Params([[要登記的型別], [該型別的元資料]])

#Descr[
重複登記同一型別拋 `InvalidOperationException`
（防止無意覆蓋；確要替換先 `Remove` 再 `Add`）。
]
""")]
	void Add(Type Type, ITypeInfo Info);

	[Doc("""
#Sum[移除一個型別。]

#Params([[要移除的型別]])

#Rtn[原本不存在返回 false]
""")]
	bool Remove(Type Type);
}