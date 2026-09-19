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
#Sum[反射建立元資料所需的成員種類。]

#Descr[
{nameof(ITypeInfoSrc)}、{nameof(ITypeInfoSrcExtn)} 的 DAM 註解都引用本常量，
改動即全包同步。

例：把 {nameof(ReflTypeInfo)} 換成別的反射實現時，
只需要改這一處常量，全包的剪裁前提就跟著一起改。
]
""")]
	internal const DynamicallyAccessedMemberTypes ReflDam
		= DynamicallyAccessedMemberTypes.Interfaces
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicFields
		| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

	[Doc($"""
#Sum[對一個型別建立元資料：分類、收集成員、找鍵值型別、建無參實例委託。]

#Descr[
例：`new {nameof(ReflTypeInfo)}(typeof(User))` 一次算出四件事，
之後 {nameof(Kind)}、{nameof(Members)}、{nameof(ElementType)}、{nameof(KeyType)}、
{nameof(CreateObject)} 全是現成的，不做惰性重算。
]

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

	[Doc($"""
#Sum[無參構造：值型別取 `default(T)`，類型別取無參構造函數。]

#Descr[
例：有公開無參構造函數的類可以建；
接口、抽象類、只有帶參構造函數的類都拋 {nameof(NotSupportedException)}，
訊息說明是哪一種情形。
]

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

	[Doc($"""
#Sum[型別分類。]

#Params([[要分類的型別]])

#Rtn[官方 {nameof(JsonTypeInfoKind)} 分類結果]

#Descr[
先剝 {nameof(Nullable<int>)}，
再按 字典、標量、集合、物件 的優先序判定，
結果映射到官方 {nameof(JsonTypeInfoKind)}
（標量對應官方的 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}，
見 {nameof(ITypeInfo.Kind)} 的說明）。

例：`i32?` 先剝成 `i32` 再判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.None)}，
故可空值型別與其非可空版本的分類相同；
`{nameof(Dictionary<string, int>)}` 判成字典而不是集合；
有成員的類落到最後一檔判成 {nameof(JsonTypeInfoKind)}.{nameof(JsonTypeInfoKind.Object)}。
]
""")]
	private static JsonTypeInfoKind ComputeKind([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
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

	[Doc($"""
#Sum[是否字典。]

#Params([[要判定的型別]])

#Rtn[是字典返回 true]

#Descr[
實現了非泛型 {nameof(System.Collections.IDictionary)}，
或（直接是或實現了）{nameof(IDictionary<,>)}。

例：`{nameof(Dictionary<string, int>)}` 兩條都命中；
只實現 `{nameof(IDictionary<,>)}` 的自訂型別命中第二條，
故自訂字典不會被誤判成集合。
]
""")]
	private static bool IsDictionary([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		// step 1: 非泛型 IDictionary 最直接。
		if(typeof(System.Collections.IDictionary).IsAssignableFrom(T)){
			return true;
		}
		// step 2: 泛型 IDictionary<,> 也算（含自訂實現）。
		return FindGenericIface(T, typeof(IDictionary<,>)) is not null;
	}

	[Doc($"""
#Sum[找集合的元素型別。]

#Params([[要查找的型別]])

#Rtn[元素型別；非集合為 null]

#Descr[
數組取 {nameof(Type.GetElementType)}；
字典取值型別（與官方 {nameof(JsonTypeInfo.ElementType)} 一致，
官方對字典的元素型別就是值型別）；
其餘取 `{nameof(IEnumerable<>)}` 的泛型實參。

例：`i32[]` 是 `typeof(i32)`；
`{nameof(Dictionary<string, int>)}` 是 `typeof(i32)`（值型別，不是鍵型別）；
`{nameof(List<string>)}` 是 `typeof(str)`；
`i32` 這類標量是 null。
]
""")]
	private static Type? FindElementType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
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

	[Doc($"""
#Sum[找字典的鍵型別。]

#Params([[要查找的型別]])

#Rtn[鍵型別；非字典為 null]

#Descr[
即 `{nameof(IDictionary<,>)}` 的第一個泛型實參。

例：`{nameof(Dictionary<string, int>)}` 的鍵型別是 `typeof(str)`；
`{nameof(List<int>)}` 不是字典，返回 null（此時 {nameof(TypeInfoBase.ElementType)} 有值、本項沒有）。
]
""")]
	private static Type? FindKeyType([DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type T){
		var I = FindGenericIface(T, typeof(IDictionary<,>));
		return I?.GetGenericArguments()[0];
	}

	[Doc($"""
#Sum[找 `T` 上實現了「泛型定義為 `Def`」的最近介面。]

#Params([[要查找的型別], [泛型介面的開放泛型定義]])

#Rtn[找到的介面型別；沒有為 null]

#Descr[
`T` 本身是該介面也認。

例：`Def` 傳 `typeof({nameof(IDictionary<,>)})` 時，
`{nameof(Dictionary<string, int>)}` 返回它自己實現的 `{nameof(IDictionary<string, int>)}`；
`{nameof(List<int>)}` 返回 null。

只認第一個匹配的介面，故有多個同定義介面時取 {nameof(Type.GetInterfaces)} 的順序（穩定）。
]
""")]
	private static Type? FindGenericIface(
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

	[Doc($"""
#Sum[收集成員：公開實例屬性（排除索引器）加公開實例字段。]

#Params([[要收集的型別]])

#Rtn[成員表（可變 {nameof(List<object>)} 作為中間結果）]

#Descr[
屬性段在前、字段段後；
段內順序 = 收集序（即 {nameof(Type.GetProperties)} 與 {nameof(Type.GetFields)} 交出的順序，
它就是可用的穩定序；
不用 {nameof(MemberInfo.MetadataToken)}：NativeAOT 的 NativeFormat 元數據不提供它，取會拋）。

類型層次的合併（基類在前）與同名遮蔽的去重由 {nameof(TypeInfoSorter)} 統一處理。
本方法返回可變 {nameof(List<object>)}，
建構子規整後才成為對外只讀契約。

例：型別上有 `public i32 Age` 屬性與 `public str Note` 字段時兩個都收；
`private` 成員、`static` 成員、`this[i32]` 索引器都不收。
]
""")]
	private static IReadOnlyList<IMemberInfo> CollectMembers(
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
			R.Add(new ReflMemberInfo(Prop));
		}
		// step 2: 公開實例字段；排在屬性段之後，構成「屬性在前、字段在後」的收集序。
		foreach(var Fld in T.GetFields(BindingFlags.Instance | BindingFlags.Public)){
			R.Add(new ReflMemberInfo(Fld));
		}
		return R;
	}

	[Doc($"""
#Sum[建立無參實例委託。]

#Params([[要建實例的型別]])

#Rtn[無參實例工廠；無構造函數、抽象、接口返回 null]

#Descr[
值型別取默認值；類型別解析無參構造函數。

返回型別與官方 {nameof(JsonTypeInfo)}.{nameof(JsonTypeInfo.CreateObject)} 一致
（{nameof(Func<object>)}，非空）。

JIT 下用表達式樹一次編譯成委託（靜態引用構造函數，剪裁友好）；
NativeAOT 不支持動態編譯
（{nameof(Expression)}.{nameof(Expression.Lambda)} 的 `Compile` 拋
{nameof(PlatformNotSupportedException)}），
退回 {nameof(Activator)}.{nameof(Activator.CreateInstance)}——
構造函數元數據已由 {nameof(ReflDam)} 保證保留。

例：`i32` 返回一個每次調用都給出 0 的委託；
接口與抽象類返回 null（故 {nameof(ITypeInfo.CanMkInst)} 為 false）；
只有帶參構造函數的類也返回 null。
]
""")]
	private static Func<obj>? TryBuildMkInst(
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