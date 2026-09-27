namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ITypeInfoSrcExtn)} 的函數實現。]

#Descr[
只放函數實現：簽名與參數特性（DAM、{nameof(NotNullWhenAttribute)}）在 `ITypeInfoSrcExtn.cs`。
`partial` 方法合併時重複標記特性會報 CS0579，故此處不重複。

用法示例一律寫在聲明側（`ITypeInfoSrcExtn.cs`），此處只留實現要點。
]
""")]
public static partial class ITypeInfoSrcExtn{
	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
本體只是原樣返回，作用是在編譯期把 DAM 註解掛上。
]
""")]
	private static partial Type _RuntimeType(Type T){
		return T;
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
三步：查型別（{nameof(ITypeInfoSrc.TryGetInfo)}），取到就原樣返回，
取不到才付「拼錯誤訊息」的代價——那是錯誤路徑。
]
""")]
	public static partial ITypeInfo GetInfo(this ITypeInfoSrc z, Type Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(Type);
		// step 1: 查得到就直接返回；查不到才付「拼錯誤訊息」的代價（錯誤路徑）。
		if(z.TryGetInfo(Type, out var Info)){
			return Info;

		}
		throw new KeyNotFoundException($"型別 {Type.FullName} 未註冊到來源 {z.GetType().Name}，取不到型別元資料。");
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：型別由 `T` 給（{nameof(_RuntimeType)} 只是把 `typeof(T)` 帶上 DAM 註解交出去）。
]
""")]
	public static partial ITypeInfo GetInfo<T>(this ITypeInfoSrc z){
		// 型別編譯期已知，故由 T 取；其餘與 typeof 版同一條路。
		return z.GetInfo(_RuntimeType(typeof(T)));
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
型別未註冊時由本方法拋（訊息指出型別與來源）；
型別在、成員名不存在時由 {nameof(ITypeInfo.GetMember)} 拋（訊息列出可用成員名）。
]
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
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：所屬型別由 `T` 給，成員名原樣傳下去。
]
""")]
	public static partial IMemberInfo GetMember<T>(this ITypeInfoSrc z, str Name){
		// 型別編譯期已知，故由 T 取；其餘與 typeof 版同一條路。
		return z.GetMember(_RuntimeType(typeof(T)), Name);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
兩級短路：來源傳 null 直接 false；型別查不到也 false；
型別內的按名查走 {nameof(ITypeInfo.TryGetMember)}（惰性索引，O(1)）。
]
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
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：所屬型別由 `T` 給，成員名與出參原樣傳下去（「未知」一律用 false 表示）。
]
""")]
	public static partial bool TryGetMember<T>(this ITypeInfoSrc z, str Name, out IMemberInfo? M){
		return z.TryGetMember(_RuntimeType(typeof(T)), Name, out M);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
查型別、按名取成員、讀值三步合一；
成員自身的讀值判據（可讀性、實例型別）由 {nameof(IMemberInfo)} 收口，兩套來源一條口徑。
]
""")]
	public static partial bool TryGet(this ITypeInfoSrc z, Type Type, obj? O, str Name, out obj? R){
		R = default;
		// 先按名取成員（含型別註冊檢查），成員取不到就沒必要再往下。
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;

		}
		// 成員自身的讀值判據由 Member 收口（兩套來源一條口徑）。
		return M.TryGet(O, out R);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：型別取 `T` 的靜態型別（不是實例的執行期型別），其餘三步（查型別、查成員、讀值）照舊。
]
""")]
	public static partial bool TryGet<T>(this ITypeInfoSrc z, T O, str Name, out obj? V){
		return z.TryGet(_RuntimeType(typeof(T)), O, Name, out V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
同上三步合一；寫值判據由 {nameof(IMemberInfo)} 收口（值型別不符照常拋）。
]
""")]
	public static partial bool TrySet(this ITypeInfoSrc z, Type Type, obj? O, str Name, obj? V){
		// 先按名取成員（含型別註冊檢查），成員取不到就沒必要再往下。
		if(!z.TryGetMember(Type, Name, out var M)){
			return false;

		}
		// 成員自身的寫值判據由 Member 收口（值型別不符照常拋）。
		return M.TrySet(O, V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：型別取 `T` 的靜態型別，其餘與 typeof 版同一條路（寫值判據仍在成員本體上）。
]
""")]
	public static partial bool TrySet<T>(this ITypeInfoSrc z, T O, str Name, obj? V){
		return z.TrySet(_RuntimeType(typeof(T)), O, Name, V);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉調帶型別的那個重載（型別傳 null，即取 `O.GetType()`）。
]
""")]
	public static partial IInstViewDict ToInstViewDict(this ITypeInfoSrc z, obj? O){
		return ToInstViewDict(z, O, null);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
型別未註冊到來源時拋（訊息指出型別與來源），不留到讀寫時才暴露。
]
""")]
	public static partial IInstViewDict ToInstViewDict(this ITypeInfoSrc z, obj? O, Type? Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(O);
		// 動態 GetType() 的 DAM 需要擔保，見 RuntimeType。
		var T = Type ?? _RuntimeType(O.GetType());
		if(!z.TryGetInfo(T, out var Info)){
			throw new KeyNotFoundException($"型別 {T.FullName} 未註冊到來源 {z.GetType().Name}，無法建字典視圖。");

		}
		return new InstViewDict(O, Info);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：型別取 `T` 的靜態型別（故 {nameof(InstViewDict)} 的鍵表以 `typeof(T)` 為準），
不傳型別的那個重載才按執行期型別走。
]
""")]
	public static partial IInstViewDict ToInstViewDict<T>(this ITypeInfoSrc z, T O){
		return z.ToInstViewDict(O, _RuntimeType(typeof(T)));
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉調帶型別的那個重載（型別傳 null，即取 `O.GetType()`）。
]
""")]
	public static partial ResAssignFromDict AssignFromDict(this ITypeInfoSrc z, obj? O, IEnumerable<KeyValuePair<str, obj?>> Dict){
		return AssignFromDict(z, O, Dict, null);
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
逐鍵四步：查型別 → 按名取成員（未知鍵當場拋）→ 只讀成員跳過 → 寫入。
未知鍵的報錯訊息同時列可寫名與可讀名，是因為「成員不存在」與「成員在但不可寫」是兩種錯。
]
""")]
	public static partial ResAssignFromDict AssignFromDict(this ITypeInfoSrc z, obj? O, IEnumerable<KeyValuePair<str, obj?>> Dict, Type? Type){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(O);
		ArgumentNullException.ThrowIfNull(Dict);
		// 動態 GetType() 的 DAM 需要擔保，見 RuntimeType。
		var T = Type ?? _RuntimeType(O.GetType());
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
					$"字典含非成員鍵 {K}；型別 {T.FullName} 的可寫名：{string.Join(", ", Info.WritableMembers.Keys)}；"
					+ $"可讀名：{string.Join(", ", Info.ReadableMembers.Keys)}"
				);
			}
			// step 3: 只讀成員按已定語義跳過，不算錯。
			if(!M.CanWrite){
				continue;
			}
			// step 4: 寫入；值型別不符時拋（那是調用方的 bug，不是「不可寫」）。
			// 原始異常由成員本體拋（反射側 ArgumentException、官方委託側 InvalidCastException），
			// 這裡統一歸成「寫回失敗」這一種，免得調用方要分別認兩套異常型別。
			try{
				if(!M.TrySet(O, V)){
					throw new InvalidOperationException($"寫入成員 {T.FullName}.{K} 失敗（值型別不符）。");
				}
			}catch(InvalidOperationException){
				throw;
			}catch(Exception E) when(E is ArgumentException or InvalidCastException){
				throw new InvalidOperationException($"寫入成員 {T.FullName}.{K} 失敗（值型別不符）。", E);
			}
		}
		return new ResAssignFromDict();
	}

	[Doc($"""
#Sum[見宣告處的說明。]

#Descr[
轉非泛型版：型別取 `T` 的靜態型別，故只認 `typeof(T)` 那張成員表。
]
""")]
	public static partial ResAssignFromDict AssignFromDict<T>(this ITypeInfoSrc z, T O, IEnumerable<KeyValuePair<str, obj?>> Dict){
		return z.AssignFromDict(O, Dict, _RuntimeType(typeof(T)));
	}

}













