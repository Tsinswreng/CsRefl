namespace Tsinswreng.CsRefl.Test.Domains.Models;

/// 沒有無參構造函數的模型：驗證反射來源的 CanMkInst=false。
/// 注意：本型別不註冊進 TestJsonCtx——STJ 源生成對無無參構造函數的物件型別
/// 會生成帶參數構造，CreateObject 為 null（無參工廠），不代表無法序列化。
public class PoNoCtor{
	/// 唯一的值。
	public i32 X{get;}

	public PoNoCtor(i32 X){
		this.X = X;
	}
}