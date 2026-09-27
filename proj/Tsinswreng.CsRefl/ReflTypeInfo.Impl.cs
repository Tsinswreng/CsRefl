namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ReflTypeInfo)} 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `ReflTypeInfo.cs`。
]
""")]
public partial class ReflTypeInfo{
	[Doc($"""
#Sum[對一個型別建立元資料：分類、收集成員、找鍵值型別、建無參實例委託。]

#See[{nameof(ReflTypeInfo)}]
""")]
	public partial ReflTypeInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type
	){
		ArgumentNullException.ThrowIfNull(Type);
		// step 1: 型別事實建構期一次算好，直接落在屬性上（自動屬性），不另存欄位由屬性轉發。
		this.Type = Type;
		this.Kind = ComputeKind(Type);
		this.ElementType = FindElementType(Type);
		this.KeyType = FindKeyType(Type);
		// step 2: 成員表在此規整成契約序（排序、去重、只讀），見 TypeInfoSorter.SortEtDedup。
		this.Members = TypeInfoSorter.SortEtDedup(Type, CollectMembers(Type));
		// step 3: 無參工廠構造期算一次就落在屬性上。
		this.CreateObject = TryBuildMkInst(Type);
	}

	[Doc($"""
#Sum[無參構造：值型別取 `default(T)`，類型別取無參構造函數。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public partial obj? MkInst(){
		// 錯誤訊息用 CreateObject 判空（與官方那條「CreateObject 是否為 null」同一判據）。
		var F = CreateObject;
		if(F is null){
			throw new NotSupportedException(
				$"型別 {Type.FullName} 沒有可用的無參構造函數（接口/抽象類/無無參構造函數），無法建立實例。"
			);
		}
		return F();
	}

	[Doc($"""
#Sum[惰性建立按名索引。]

#Descr[
成員表在建構後不可變，故緩存安全。
索引雙檢：{nameof(_ByName)} 是 volatile，兩個線程同時建也只會多建一份等價字典。

實測：第一次按名查時才建這份字典，故「只枚舉成員、從不按名查」的用法不付這份內存代價；
建好之後每次按名查是 O(1)。

鍵比較用 {nameof(StringComparer)}.{nameof(StringComparer.Ordinal)} 而非默認比較：
成員名是程式碼識別符，Ordinal 才是正確語義，也不受當前文化影響。
]
""")]
	private partial void EnsureByName(){
		if(_ByName is not null){
			return;
		}
		var Dict = new Dictionary<str, IMemberInfo>(Members.Count, StringComparer.Ordinal);
		foreach(var M in Members){
			Dict[M.Name] = M;
		}
		_ByName = Dict;
	}

	[Doc($"""
#Sum[按成員序列出全部成員名，供未命中時的錯誤訊息用。]

#Descr[
實測（`PoUser`）：11 個名，與成員表同序。
]
""")]
	private partial IEnumerable<str> AllNames(){
		return Members.Select(M => M.Name);
	}

	[Doc($"""
#Sum[按名查成員；走惰性索引，O(1)。]

#See[{nameof(ITypeInfo.TryGetMember)}]
""")]
	public partial bool TryGetMember(str Name, out IMemberInfo? M){
		M = null;
		// step 1: 名字為 null 時直接返回 false（成員名不可能是 null，故這不是「查不到」而是「沒法查」）。
		if(Name is null){
			return false;
		}
		// step 2: 走索引（第一次調用時才建，見 EnsureByName）。
		EnsureByName();
		return _ByName!.TryGetValue(Name, out M);
	}

	[Doc($"""
#Sum[按名取成員；未知拋 {nameof(KeyNotFoundException)}。]

#See[{nameof(ITypeInfo.GetMember)}]
""")]
	public partial IMemberInfo GetMember(str Name){
		ArgumentNullException.ThrowIfNull(Name);
		// step 1: 命中就返回；未命中才付「列可用名」的代價（錯誤路徑）。
		if(TryGetMember(Name, out var M)){
			return M;
		}
		throw new KeyNotFoundException(
			$"型別 {Type.FullName} 沒有成員 {Name}。可用成員：{string.Join(", ", AllNames())}"
		);
	}

	public partial bool TryGetMemberType(str Name, out Type? T){
		throw new NotImplementedException();
	}

	public partial bool CanRead(str Name){
		throw new NotImplementedException();
	}

	public partial bool CanWrite(str Name){
		throw new NotImplementedException();
	}

	[Doc($"""
#Sum[型別分類。]

#See[{nameof(ITypeInfo.Kind)}]
""")]
	private static partial JsonTypeInfoKind ComputeKind([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		// step 1: 剝掉 Nullable，可空值型別的分類與其非可空版本一致。
		var U = Nullable.GetUnderlyingType(T);
		if(U is not null){
			return ComputeKind(U);
		}
		// step 2: 字典優先（字典也實現 IEnumerable，故必須排在集合之前判）。
		if(IsDictionary(T)){
			return JsonTypeInfoKind.Dictionary;
		}
		// step 3: 標量（枚舉、基元、字符串、常用 BCL 值型別）對應官方的 None。
		if(T.IsEnum || T.IsPrimitive || T == typeof(str) || T == typeof(decimal)
			|| T == typeof(DateTime) || T == typeof(DateTimeOffset) || T == typeof(TimeSpan)
			|| T == typeof(Guid) || T == typeof(DateOnly) || T == typeof(TimeOnly))
		{
			return JsonTypeInfoKind.None;
		}
		// step 4: 其餘可枚舉的就是集合。
		if(typeof(System.Collections.IEnumerable).IsAssignableFrom(T)){
			return JsonTypeInfoKind.Enumerable;
		}
		// step 5: 剩下的一律當物件（有成員的類、結構、記錄等）。
		return JsonTypeInfoKind.Object;
	}

	private static partial bool IsDictionary([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		// step 1: 非泛型 IDictionary 最直接。
		if(typeof(System.Collections.IDictionary).IsAssignableFrom(T)){
			return true;
		}
		// step 2: 泛型 IDictionary<,> 也算（含自訂實現）。
		return FindGenericIface(T, typeof(IDictionary<,>)) is not null;
	}

	private static partial Type? FindElementType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		// step 1: 數組自己就帶元素型別。
		if(T.IsArray){
			return T.GetElementType();
		}
		// step 2: 字典取值型別的泛型實參（與官方 JsonTypeInfo.ElementType 口徑一致）。
		var D = FindGenericIface(T, typeof(IDictionary<,>));
		if(D is not null){
			return D.GetGenericArguments()[1];
		}
		// step 3: 其餘取 IEnumerable<T> 的泛型實參；標量沒有故返回 null。
		var I = FindGenericIface(T, typeof(IEnumerable<>));
		return I?.GetGenericArguments()[0];
	}

	private static partial Type? FindKeyType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		var I = FindGenericIface(T, typeof(IDictionary<,>));
		return I?.GetGenericArguments()[0];
	}

	private static partial Type? FindGenericIface(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type T,
		Type Def
	){
		// step 1: T 本身就是該泛型介面時直接認它。
		if(T.IsGenericType && T.GetGenericTypeDefinition() == Def){
			return T;
		}
		// step 2: 否則在已實現的介面裏找第一個匹配的。
		return T.GetInterfaces().FirstOrDefault(
			X => X.IsGenericType && X.GetGenericTypeDefinition() == Def
		);
	}

	private static partial IReadOnlyList<IMemberInfo> CollectMembers(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
		)] Type T
	){
		var R = new List<IMemberInfo>();
		// step 1: 公開實例屬性；索引器要排除（它需要下標，不是可按名讀寫的成員）。
		foreach(var Prop in T.GetProperties(BindingFlags.Instance | BindingFlags.Public)){
			if(Prop.GetIndexParameters().Length > 0){
				continue;
			}
			// 成員由 ReflMemberInfo 包住官方 PropertyInfo；
			// 取名字、宣告型別、可讀可寫一律由 IMemberInfo 收口。
			R.Add(new ReflMemberInfo(Prop));
		}
		// step 2: 公開實例字段；排在屬性段之後，構成「屬性在前、字段在後」的收集序。
		foreach(var Fld in T.GetFields(BindingFlags.Instance | BindingFlags.Public)){
			R.Add(new ReflMemberInfo(Fld));
		}
		return R;
	}

	private static partial Func<obj>? TryBuildMkInst(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
			| DynamicallyAccessedMemberTypes.Interfaces
		)] Type T
	){
		// step 1: 抽象類與接口建不出實例。
		if(T.IsAbstract || T.IsInterface){
			return null;
		}
		// step 2: 值型別：CreateInstance 直接產生 boxed 默認值，無需構造函數元數據。
		// 值型別的 CreateInstance 結果必然非 null，故用 ! 收口可空性。
		if(T.IsValueType){
			return () => Activator.CreateInstance(T)!;
		}
		// step 3: 類型別：沒有無參構造函數就不可建。
		if(T.GetConstructor(Type.EmptyTypes) is null){
			return null;
		}
		try{
			// step 4a: JIT 路徑：表達式樹一次編譯，之後每次調用只是普通委託調用。
			var Ctor = T.GetConstructor(Type.EmptyTypes)!;
			Expression Body = Expression.New(Ctor);
			var Boxed = Expression.Convert(Body, typeof(obj));
			return Expression.Lambda<Func<obj>>(Boxed).Compile();
		}
		catch(PlatformNotSupportedException){
			// step 4b: NativeAOT：退回反射創建，每次調用走一次 Activator
			//（MkInst 頻率低，可接受）。
			return () => Activator.CreateInstance(T)!;
		}
	}
}
