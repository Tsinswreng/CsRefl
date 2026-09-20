namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

public static partial class ITypeInfoExtn{
	public static partial bool TryGetMember(this ITypeInfo z, str Name, out obj? M){
		M = null;
		if(z is null || Name is null){
			return false;
		}
		// step 1: 按名查就是掃成員表比官方成員名；兩側官方成員名由 MemberExtn.Name 收口。
		foreach(var Item in z.Members){
			if(MemberExtn.Name(Item) == Name){
				M = Item;
				return true;
			}
		}
		return false;
	}

	public static partial obj? GetMember(this ITypeInfo z, str Name){
		ArgumentNullException.ThrowIfNull(z);
		// step 1: 命中就返回，未命中才付「列可用名」的代價。
		if(TryGetMember(z, Name, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {z.Type.FullName} 沒有成員 {Name}。可用成員：{string.Join(", ", z.Members.Select(X => MemberExtn.Name(X)))}"
		);
	}

	public static partial bool TryGet(this ITypeInfo z, str Name, obj? O, out obj? V){
		V = default;
		// step 1: 按名取成員（含存在性檢查），取不到就沒必要再往下。
		if(!TryGetMember(z, Name, out var M)){
			return false;
		}
		// step 2: 讀值；成員自身能力與實例型別由 MemberExtn 收口。
		return MemberExtn.TryGet(M, O, out V);
	}

	public static partial bool TrySet(this ITypeInfo z, str Name, obj? O, obj? V){
		// step 1: 同上。
		if(!TryGetMember(z, Name, out var M)){
			return false;
		}
		// step 2: 寫值；值型別不符照常拋。
		return MemberExtn.TrySet(M, O, V);
	}

	public static partial IReadOnlyCollection<str> ReadableNames(this ITypeInfo z){
		// step 1: 判據是「成員能不能讀」，不是「在不在某張鍵表裏」。
		return z.Members.Where(M => MemberExtn.CanRead(M)).Select(M => MemberExtn.Name(M)).ToList();
	}

	public static partial IReadOnlyCollection<str> WritableNames(this ITypeInfo z){
		return z.Members.Where(M => MemberExtn.CanWrite(M)).Select(M => MemberExtn.Name(M)).ToList();
	}

	public static partial bool CanMkInst(this ITypeInfo z){
		// step 1: 兩側各問各的官方工廠：Json 側直接問官方，反射側問反射算出的工廠。
		return z.CreateObjectOf() is not null;
	}

	public static partial obj? MkInst(this ITypeInfo z){
		var F = z.CreateObjectOf()
			?? throw new NotSupportedException(
				$"型別 {z.Type.FullName} 沒有可用的無參構造函數（接口／抽象類／無參構造函數缺失），無法建立實例。"
			);
		return F();
	}

	[Doc($"""
#Sum[本型別是否已有成員表中的該名字（供內部與擴展複用）。]

#Params([[z, 型別元資料], [Name, 成員名]])

#Rtn[在成員表裏返回 true]
""")]
	private static partial bool HasMember(this ITypeInfo z, str Name);
}