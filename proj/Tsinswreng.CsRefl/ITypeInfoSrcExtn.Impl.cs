namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`ITypeInfoSrcExtn` 的函數實現。]

#Descr[
只放函數實現：簽名與參數特性（DAM/`NotNullWhen`）在 `ITypeInfoSrcExtn.cs`。
`partial` 方法合併時重複標記特性會報 CS0579，故此處不重複。
]
""")]
public static partial class ITypeInfoSrcExtn{
	[Doc("""
#Sum[見宣告處的說明。]

#See[{nameof(ITypeInfoSrcExtn.RuntimeType)}]
""")]
	private static partial Type RuntimeType(Type T){
		return T;
	}

	[Doc("""
#Sum[取成員；型別未註冊或成員不存在都拋 `KeyNotFoundException`（訊息含線索）。]

#See[{nameof(ITypeInfoSrcExtn.GetMember)}]
""")]
	public static partial IMemberInfo GetMember(this ITypeInfoSrc z, Type Type, str Name){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(Type);
		if(z.TryGetInfo(Type, out var Info)){
			return Info.GetMember(Name);
		}
		throw new KeyNotFoundException($"型別 {Type.FullName} 未註冊到來源 {z.GetType().Name}，無法取成員 {Name}。");
	}

	[Doc("""
#Sum[Try 版：型別未註冊 / 成員不存在 / 入參為 null 都返回 false。]

#See[{nameof(ITypeInfoSrcExtn.TryGetMember)}]
""")]
	public static partial bool TryGetMember(this ITypeInfoSrc z, Type Type, str Name, out IMemberInfo? M){
		M = null;
		if(z is null || Type is null){
			return false;
		}
		if(!z.TryGetInfo(Type, out var Info)){
			return false;
		}
		return Info.TryGetMember(Name, out M);
	}

	[Doc("""
#Sum[按名讀值。]

#See[{nameof(ITypeInfoSrcExtn.TryGet)}]
""")]
	public static partial bool TryGet(this ITypeInfoSrc z, Type Type, str Name, obj? O, out obj? R){
		R = default;
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;
		}
		return M.TryGet(O, out R);
	}

	[Doc("""
#Sum[按名寫值。]

#See[{nameof(ITypeInfoSrcExtn.TrySet)}]
""")]
	public static partial bool TrySet(this ITypeInfoSrc z, Type Type, str Name, obj? O, obj? V){
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;
		}
		return M.TrySet(O, V);
	}

	[Doc("""
#Sum[建淺字典視圖，型別取 `O.GetType()`。]

#See[{nameof(ITypeInfoSrcExtn.ToInstDict)}]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O){
		return ToInstDict(z, O, null);
	}

	[Doc("""
#Sum[建淺字典視圖，型別可由調用方顯式給。]

#See[{nameof(ITypeInfoSrcExtn.ToInstDict)}]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O, Type? Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(O);
		// 動態 GetType() 的 DAM 需要擔保，見 RuntimeType。
		var T = Type ?? RuntimeType(O.GetType());
		if(!z.TryGetInfo(T, out var Info)){
			throw new KeyNotFoundException($"型別 {T.FullName} 未註冊到來源 {z.GetType().Name}，無法建字典視圖。");
		}
		return new InstDict(O, Info);
	}

	[Doc("""
#Sum[把字典寫回物件，型別取 `O.GetType()`。]

#See[{nameof(ITypeInfoSrcExtn.AssignFromDict)}]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict){
		AssignFromDict(z, O, Dict, null);
	}

	[Doc("""
#Sum[把字典寫回物件，型別可由調用方顯式給。]

#See[{nameof(ITypeInfoSrcExtn.AssignFromDict)}]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict, Type? Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(O);
		ArgumentNullException.ThrowIfNull(Dict);
		// 動態 GetType() 的 DAM 需要擔保，見 RuntimeType。
		var T = Type ?? RuntimeType(O.GetType());
		if(!z.TryGetInfo(T, out var Info)){
			throw new KeyNotFoundException($"型別 {T.FullName} 未註冊到來源 {z.GetType().Name}，無法執行字典寫回。");
		}
		foreach(var (K, V) in Dict){
			if(!Info.TryGetMember(K, out var M)){
				// 未知鍵的報錯訊息同時列可寫名與可讀名：未知鍵的判據是整張成員表，
				// 只列可寫名會讓「成員存在但不可寫」的調用方找不到線索。
				throw new KeyNotFoundException(
					$"字典含非成員鍵 {K}；型別 {T.FullName} 的可寫名：{string.Join(", ", Info.WritableNames)}；"
					+ $"可讀名：{string.Join(", ", Info.ReadableNames)}"
				);
			}
			// 只讀成員按已定語義跳過，不算錯。
			if(!M.CanWrite){
				continue;
			}
			if(!M.TrySet(O, V)){
				throw new InvalidOperationException($"寫入成員 {T.FullName}.{K} 失敗（值型別不符）。");
			}
		}
	}
}