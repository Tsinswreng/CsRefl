namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;

/// ITypeInfoSrcExtn 的函數實現。
/// 參數特性（DAM/NotNullWhen）只寫在聲明文件（ITypeInfoSrcExtn.cs），
/// partial 方法合併時重複標記會報 CS0579。
public static partial class ITypeInfoSrcExtn{
	/// 運行期型別的 DAM 擔保：動態 GetType() 在分析器眼裏不攜帶 DAM 信息，但本包
	/// 在此路徑上對 T 的用法只有 IsInstanceOfType / 反射建元資料，缺元數據時會在
	/// 反射建元資料處自然拋錯，不會悄悄剪錯，故此處顯式擔保成員元數據需求。
	[UnconditionalSuppressMessage("Trimming", "IL2068",
		Justification = "運行期型別（O.GetType()）本質無法靜態攜帶 DAM 信息；本包對它的用法只有 IsInstanceOfType 與反射建元資料，缺元數據時在反射建元資料處自然拋錯，不會悄悄剪錯。")]
	[return: DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)]
	private static Type RuntimeType(Type T){
		return T;
	}

	public static partial IMemberInfo GetMember(this ITypeInfoSrc z, Type Type, str CodeName){
		ArgumentNullException.ThrowIfNull(z);
		ArgumentNullException.ThrowIfNull(Type);
		if(z.TryGetInfo(Type, out var Info)){
			return Info.GetMember(CodeName);
		}
		throw new KeyNotFoundException($"型別 {Type.FullName} 未註冊到來源 {z.GetType().Name}，無法取成員 {CodeName}。");
	}

	public static partial bool TryGetMember(this ITypeInfoSrc z, Type Type, str CodeName, out IMemberInfo? M){
		M = null;
		if(z is null || Type is null){
			return false;
		}
		if(!z.TryGetInfo(Type, out var Info)){
			return false;
		}
		return Info.TryGetMember(CodeName, out M);
	}

	public static partial bool TryGet(this ITypeInfoSrc z, Type Type, str CodeName, obj? O, out obj? R){
		R = default;
		if(!z.TryGetMember(Type, CodeName, out var M)){
			return false;
		}
		return M.TryGet(O, out R);
	}

	public static partial bool TrySet(this ITypeInfoSrc z, Type Type, str CodeName, obj? O, obj? V){
		if(!z.TryGetMember(Type, CodeName, out var M)){
			return false;
		}
		return M.TrySet(O, V);
	}

	public static partial IInstDict ToInstDict(this ITypeInfoSrc z, obj? O){
		return ToInstDict(z, O, null);
	}

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

	public static partial void AssignFromDict(this ITypeInfoSrc z, obj? O, IReadOnlyDictionary<str, obj?> Dict){
		AssignFromDict(z, O, Dict, null);
	}

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
				throw new KeyNotFoundException($"字典含非成員鍵 {K}；型別 {T.FullName} 的可用鍵：{string.Join(", ", Info.WritableNames)}");
			}
			if(!M.CanWrite){
				continue;
			}
			if(!M.TrySet(O, V)){
				throw new InvalidOperationException($"寫入成員 {T.FullName}.{K} 失敗（值型別不符）。");
			}
		}
	}
}