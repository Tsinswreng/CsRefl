namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ITypeInfoSrcExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名與參數特性（DAM、{nameof(NotNullWhenAttribute)}）在 `ITypeInfoSrcExtn.cs`。
`partial` 方法合併時重複標記特性會報 CS0579，故此處不重複。
]
""")]
public static partial class ITypeInfoSrcExtn{
	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
例：本體只是原樣返回，作用是在編譯期把 DAM 註解掛上，
故調用方拿到的 {nameof(Type)} 在分析器眼裏就帶了成員元數據擔保。
]

#See[{nameof(ITypeInfoSrcExtn.RuntimeType)}]
""")]
	private static partial Type RuntimeType(Type T){
		return T;
	}

	[Doc($"""
#Sum[取成員；型別未註冊或成員不存在都拋 {nameof(KeyNotFoundException)}（訊息含線索）。]

#Descr[
例：型別未註冊時訊息說明「未註冊到來源 Xxx」；
型別在、成員名不存在時由 {nameof(ITypeInfo.GetMember)} 拋，訊息列出可用成員名。
兩種失敗分得開，排查時不必猜是哪一種。
]

#See[{nameof(ITypeInfoSrcExtn.GetMember)}]
""")]
	public static partial IMemberInfo GetMember(this ITypeInfoSrc z, Type Type, str Name){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(Type);
		// step 1: 型別要能查到，否則這一步就沒法繼續（訊息指出是哪個來源）。
		if(z.TryGetInfo(Type, out var Info)){
			// step 2: 型別內的按名查詢交給類型元資料自己（它會給出可用成員名）。
			return Info.GetMember(Name);
		}
		throw new KeyNotFoundException($"型別 {Type.FullName} 未註冊到來源 {z.GetType().Name}，無法取成員 {Name}。");
	}

	[Doc($"""
#Sum[Try 版：型別未註冊、成員不存在、入參為 null 都返回 false。]

#Descr[
例：`Src.{nameof(TryGetMember)}(null, "Age", out _)` 返回 false 而不拋，
故可以放心接外部傳來的型別與名字。
]

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

	[Doc($"""
#Sum[按名讀值。]

#Descr[
例：`Src.{nameof(TryGet)}(typeof(User), "Age", User, out var V)` 命中；
讀只寫成員返回 false（成員在、可讀性不滿足）。
]

#See[{nameof(ITypeInfoSrcExtn.TryGet)}]
""")]
	public static partial bool TryGet(this ITypeInfoSrc z, Type Type, str Name, obj? O, out obj? R){
		R = default;
		// 先按名取成員（含型別註冊檢查），成員取不到就沒必要再往下。
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;
		}
		return M.TryGet(O, out R);
	}

	[Doc($"""
#Sum[按名寫值。]

#Descr[
例：`Src.{nameof(TrySet)}(typeof(User), "Age", User, 31)` 命中並寫回；
寫只讀成員返回 false；值型別不符照常拋。
]

#See[{nameof(ITypeInfoSrcExtn.TrySet)}]
""")]
	public static partial bool TrySet(this ITypeInfoSrc z, Type Type, str Name, obj? O, obj? V){
		// 先按名取成員（含型別註冊檢查），成員取不到就沒必要再往下。
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;
		}
		return M.TrySet(O, V);
	}

	[Doc($"""
#Sum[建淺字典視圖，型別取 `O.GetType()`。]

#Descr[
例：`Src.{nameof(ToInstDict)}(User)` 等價於傳入 null 型別的重載，
即走運行期型別。
]

#See[{nameof(ITypeInfoSrcExtn.ToInstDict)}]
""")]
	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O){
		return ToInstDict(z, O, null);
	}

	[Doc($"""
#Sum[建淺字典視圖，型別可由調用方顯式給。]

#Descr[
例：`Src.{nameof(ToInstDict)}(User, typeof(Base))` 建的視圖只認基類成員；
型別未註冊到來源時拋 {nameof(KeyNotFoundException)}，訊息說明是哪個型別與哪個來源。
]

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

	[Doc($"""
#Sum[把字典寫回物件，型別取 `O.GetType()`。]

#Descr[
例：`Src.{nameof(AssignFromDict)}(User, Dict)` 等價於傳入 null 型別的重載，
即走運行期型別。
]

#See[{nameof(ITypeInfoSrcExtn.AssignFromDict)}]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict){
		AssignFromDict(z, O, Dict, null);
	}

	[Doc($"""
#Sum[把字典寫回物件，型別可由調用方顯式給。]

#Descr[
例：字典裏有 `Age` 與一個只讀成員的鍵時，
`Age` 正常寫入、只讀那個被靜默跳過；
字典裏多一個型別上沒有的鍵時拋 {nameof(KeyNotFoundException)}，
訊息同時列可寫名與可讀名，便於對照是哪邊對不上。
]

#See[{nameof(ITypeInfoSrcExtn.AssignFromDict)}]
""")]
	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict, Type? Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(O);
		ArgumentNullException.ThrowIfNull(Dict);
		// 動態 GetType() 的 DAM 需要擔保，見 RuntimeType。
		var T = Type ?? RuntimeType(O.GetType());
		// step 1: 型別要能查到（訊息指出是哪個型別與哪個來源）。
		if(!z.TryGetInfo(T, out var Info)){
			throw new KeyNotFoundException($"型別 {T.FullName} 未註冊到來源 {z.GetType().Name}，無法執行字典寫回。");
		}
		foreach(var (K, V) in Dict){
			// step 2: 未知鍵直接拋。
			if(!Info.TryGetMember(K, out var M)){
				// 未知鍵的報錯訊息同時列可寫名與可讀名：未知鍵的判據是整張成員表，
				// 只列可寫名會讓「成員存在但不可寫」的調用方找不到線索。
				throw new KeyNotFoundException(
					$"字典含非成員鍵 {K}；型別 {T.FullName} 的可寫名：{string.Join(", ", Info.WritableNames)}；"
					+ $"可讀名：{string.Join(", ", Info.ReadableNames)}"
				);
			}
			// step 3: 只讀成員按已定語義跳過，不算錯。
			if(!M.CanWrite){
				continue;
			}
			// step 4: 寫入；值型別不符時拋（那是調用方的 bug，不是「不可寫」）。
			if(!M.TrySet(O, V)){
				throw new InvalidOperationException($"寫入成員 {T.FullName}.{K} 失敗（值型別不符）。");
			}
		}
	}
}