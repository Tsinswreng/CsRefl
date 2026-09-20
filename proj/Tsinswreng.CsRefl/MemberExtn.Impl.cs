namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(MemberExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名在 `MemberExtn.Decl.cs`。

這裡是全包唯一「分辨兩套來源成員」的地方：
官方兩側沒有共同成員型別，故用 {nameof(MemberInfo)} 與 {nameof(JsonPropertyInfo)} 的模式匹配收口，
調用方因此不必自己判來源。
]
""")]
public static partial class MemberExtn{
	public static partial str Name(this obj? M){
		// step 1: 兩側官方成員名不同，這裡是唯一的統一入口。
		return M switch{
			MemberInfo R => R.Name,
			JsonPropertyInfo J => J.Name,
			_ => "",
		};
	}

	public static partial Type? DeclaringType(this obj? M){
		// step 1: 同上，宣告型別也各取各的。
		return M switch{
			MemberInfo R => R.DeclaringType,
			JsonPropertyInfo J => J.DeclaringType,
			_ => null,
		};
	}

	public static partial bool CanRead(this obj? M){
		// 兩側各用官方自己的那條判據，不引入第二套口徑。
		return M switch{
			// Json 側：官方本來就用 Get 是否為 null 表示可讀。
			JsonPropertyInfo J => J.Get is not null,
			// 反射側：不用 PropertyInfo.CanRead（屬性帶私有 get 時它仍為 true，
			// 會把只寫屬性誤判成可讀），改問有沒有公開訪問器。
			PropertyInfo P => P.GetGetMethod() is not null,
			// 常量字段取不到實例值。
			FieldInfo F => !F.IsLiteral,
			_ => false,
		};
	}

	public static partial bool CanWrite(this obj? M){
		return M switch{
			JsonPropertyInfo J => J.Set is not null,
			PropertyInfo P => P.GetSetMethod() is not null,
			FieldInfo F => !F.IsLiteral && !F.IsInitOnly,
			_ => false,
		};
	}

	public static partial bool TryGet(this obj? M, obj? O, out obj? V){
		V = default;
		// step 1: 前置檢查——不可讀或沒有實例就沒有可讀的東西。
		if(O is null || !M.CanRead()){
			return false;
		}
		// step 2: 成員宣告在基類而實例是子類時判定為合格（繼承場景的正常用法）。
		var D = M.DeclaringType();
		if(D is not null && !D.IsInstanceOfType(O)){
			return false;
		}
		// step 3: 取值；兩側各自的官方取法。
		switch(M){
			case JsonPropertyInfo J:
				var GetFn = J.Get;
				if(GetFn is null){
					return false;
				}
				V = GetFn(O);
				return true;
			case PropertyInfo P:
				V = P.GetValue(O);
				return true;
			case FieldInfo F:
				V = F.GetValue(O);
				return true;
			default:
				return false;
		}
	}

	public static partial bool TrySet(this obj? M, obj? O, obj? V){
		// step 1: 前置檢查——不可寫或沒有實例就沒有可寫的目標。
		if(O is null || !M.CanWrite()){
			return false;
		}
		// step 2: 同上，繼承成員對子類實例合法。
		var D = M.DeclaringType();
		if(D is not null && !D.IsInstanceOfType(O)){
			return false;
		}
		// step 3: 寫值；值型別不符照常拋（那是調用方的 bug，不是「不可寫」），故不吞異常。
		switch(M){
			case JsonPropertyInfo J:
				var SetFn = J.Set;
				if(SetFn is null){
					return false;
				}
				SetFn(O, V);
				return true;
			case PropertyInfo P:
				P.SetValue(O, V);
				return true;
			case FieldInfo F:
				F.SetValue(O, V);
				return true;
			default:
				return false;
		}
	}
}