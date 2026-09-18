namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[`ReflTypeInfo` 的函數實現。]

#Descr[
只放函數實現：型別事實字段與訪問器在 `ReflTypeInfo.cs`。
]
""")]
public partial class ReflTypeInfo{
	[Doc("""
#Sum[反射建立元資料所需的成員種類。]

#Descr[
`ITypeInfoSrc` / `ITypeInfoSrcExtn` 的 DAM 註解都引用本常量，
改動即全包同步。
]
""")]
	internal const DynamicallyAccessedMemberTypes ReflDam
		= DynamicallyAccessedMemberTypes.Interfaces
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicFields
		| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

	[Doc("""
#Sum[對一個型別建立元資料：分類、收集成員、找鍵值型別、建無參實例委託。]

#See[{nameof(ReflTypeInfo)}]
""")]
	public partial ReflTypeInfo(Type Type)
		: base(
			Type: Type,
			Kind: ComputeKind(Type),
			Members: CollectMembers(Type),
			ElementType: FindElementType(Type),
			KeyType: FindKeyType(Type)
		)
	{
		// 成員的排序與去重統一交給 TypeInfoBase 建構子（見 TypeInfoSorter.SortEtDedup）。
		_mkInstFn = TryBuildMkInst(Type);
	}

	[Doc("""
#Sum[無參構造：值型別取 `default(T)`，類型別取無參構造函數。]

#See[{nameof(ITypeInfo.MkInst)}]
""")]
	public override partial obj? MkInst(){
		// 錯誤訊息用 _mkInstFn 判空（與官方那條「CreateObject 是否為 null」同一判據）。
		if(_mkInstFn is null){
			throw new NotSupportedException(
				$"型別 {Type.FullName} 沒有可用的無參構造函數（接口/抽象類/無無參構造函數），無法建立實例。"
			);
		}
		return _mkInstFn();
	}

	[Doc("""
#Sum[型別分類。]

#Params([[要分類的型別]])

#Rtn[官方 `JsonTypeInfoKind` 分類結果]

#Descr[
先剝 `Nullable`，
再按 字典→標量→集合→物件 的優先序判定，
結果映射到官方 `JsonTypeInfoKind`
（標量對應官方的 `None`，見 `ITypeInfo.Kind` 的說明）。
]
""")]
	private static JsonTypeInfoKind ComputeKind([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		var U = Nullable.GetUnderlyingType(T);
		if(U is not null){
			return ComputeKind(U);
		}
		if(IsDictionary(T)){
			return JsonTypeInfoKind.Dictionary;
		}
		if(T.IsEnum || T.IsPrimitive || T == typeof(str) || T == typeof(decimal)
			|| T == typeof(DateTime) || T == typeof(DateTimeOffset) || T == typeof(TimeSpan)
			|| T == typeof(Guid) || T == typeof(DateOnly) || T == typeof(TimeOnly))
		{
			return JsonTypeInfoKind.None;
		}
		if(typeof(System.Collections.IEnumerable).IsAssignableFrom(T)){
			return JsonTypeInfoKind.Enumerable;
		}
		return JsonTypeInfoKind.Object;
	}

	[Doc("""
#Sum[是否字典。]

#Params([[要判定的型別]])

#Rtn[是字典返回 true]

#Descr[
實現了非泛型 `IDictionary`，
或（直接是/實現了）`IDictionary<,>`。
]
""")]
	private static bool IsDictionary([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		if(typeof(System.Collections.IDictionary).IsAssignableFrom(T)){
			return true;
		}
		return FindGenericIface(T, typeof(IDictionary<,>)) is not null;
	}

	[Doc("""
#Sum[找集合的元素型別。]

#Params([[要查找的型別]])

#Rtn[元素型別；非集合為 null]

#Descr[
數組取 `GetElementType`；
字典取值型別（與 `JsonTypeInfo.ElementType` 一致，STJ 對字典的元素型別就是值型別）；
其餘取 `IEnumerable<T>` 的泛型實參。
]
""")]
	private static Type? FindElementType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		if(T.IsArray){
			return T.GetElementType();
		}
		var D = FindGenericIface(T, typeof(IDictionary<,>));
		if(D is not null){
			return D.GetGenericArguments()[1];
		}
		var I = FindGenericIface(T, typeof(IEnumerable<>));
		return I?.GetGenericArguments()[0];
	}

	[Doc("""
#Sum[找字典的鍵型別。]

#Params([[要查找的型別]])

#Rtn[鍵型別；非字典為 null]

#Descr[
即 `IDictionary<K,V>` 的第一個泛型實參。
]
""")]
	private static Type? FindKeyType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		var I = FindGenericIface(T, typeof(IDictionary<,>));
		return I?.GetGenericArguments()[0];
	}

	[Doc("""
#Sum[找 `T` 上實現了「泛型定義為 `Def`」的最近介面。]

#Params([[要查找的型別], [泛型介面的開放泛型定義]])

#Rtn[找到的介面型別；沒有為 null]

#Descr[
`T` 本身是該介面也認。
]
""")]
	private static Type? FindGenericIface(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type T,
		Type Def
	){
		if(T.IsGenericType && T.GetGenericTypeDefinition() == Def){
			return T;
		}
		return T.GetInterfaces().FirstOrDefault(
			X => X.IsGenericType && X.GetGenericTypeDefinition() == Def
		);
	}

	[Doc("""
#Sum[收集成員：公開實例屬性（排除索引器）+ 公開實例字段。]

#Params([[要收集的型別]])

#Rtn[成員表（可變 List 作為中間結果）]

#Descr[
屬性段在前、字段段後；
段內順序 = 收集序（即 `GetProperties`/`GetFields` 交出的順序，它就是可用的穩定序；
不用 `MetadataToken`：NativeAOT 的 NativeFormat 元數據不提供它，取會拋）。

類型層次的合併（基類在前）與同名遮蔽的去重由 `TypeInfoSorter` 統一處理。
本方法返回可變 `List`，
建構子規整後才成為對外只讀契約。
]
""")]
	private static IReadOnlyList<IMemberInfo> CollectMembers(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
		)] Type T
	){
		var R = new List<IMemberInfo>();
		foreach(var Prop in T.GetProperties(BindingFlags.Instance | BindingFlags.Public)){
			if(Prop.GetIndexParameters().Length > 0){
				continue;
			}
			R.Add(new ReflMemberInfo(Prop));
		}
		foreach(var Fld in T.GetFields(BindingFlags.Instance | BindingFlags.Public)){
			R.Add(new ReflMemberInfo(Fld));
		}
		return R;
	}

	[Doc("""
#Sum[建立無參實例委託。]

#Params([[要建實例的型別]])

#Rtn[無參實例工廠；無構造函數/抽象/接口返回 null]

#Descr[
值型別取默認值；類型別解析無參構造函數。

返回型別與官方 `JsonTypeInfo.CreateObject` 一致（`Func<object>`，非空）。

JIT 下用表達式樹一次編譯成委託（靜態引用構造函數，剪裁友好）；
NativeAOT 不支持動態編譯（`Compile` 拋 `PlatformNotSupportedException`），
退回 `Activator.CreateInstance`——
構造函數元數據已由 `ReflDam` 保證保留。
]
""")]
	private static Func<obj>? TryBuildMkInst(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
			| DynamicallyAccessedMemberTypes.Interfaces
		)] Type T
	){
		// 抽象類與接口建不出實例。
		if(T.IsAbstract || T.IsInterface){
			return null;
		}
		// 值型別：CreateInstance 直接產生 boxed 默認值，無需構造函數元數據。
		// 值型別的 CreateInstance 結果必然非 null，故用 ! 收口可空性。
		if(T.IsValueType){
			return () => Activator.CreateInstance(T)!;
		}
		// 類型別：沒有無參構造函數就不可建。
		if(T.GetConstructor(Type.EmptyTypes) is null){
			return null;
		}
		try{
			var Ctor = T.GetConstructor(Type.EmptyTypes)!;
			Expression Body = Expression.New(Ctor);
			var Boxed = Expression.Convert(Body, typeof(obj));
			return Expression.Lambda<Func<obj>>(Boxed).Compile();
		}
		catch(PlatformNotSupportedException){
			// NativeAOT：退回反射創建，每次調用走一次 Activator（MkInst 頻率低，可接受）。
			return () => Activator.CreateInstance(T)!;
		}
	}
}