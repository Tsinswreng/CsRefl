namespace Tsinswreng.CsRefl;

/// 可寫的型別元資料註冊表：在 ITypeInfoSrc 之上補上手動增刪。
/// 與舊 Srefl 的「可寫字典」不同：這裡註冊的內容是 ITypeInfo（元資料對象），
/// 而不是一張可以被任意替換的字典；合併與優先級交給 MergedTypeInfoSrc。
public interface ITypeInfoReg:ITypeInfoSrc{
	/// 登記一個型別的元資料。重複登記同一型別拋 InvalidOperationException
	/// （防止無意覆蓋；確要替換先 Remove 再 Add）。
	void Add(Type Type, ITypeInfo Info);
	/// 移除一個型別；原本不存在返回 false。
	bool Remove(Type Type);
}