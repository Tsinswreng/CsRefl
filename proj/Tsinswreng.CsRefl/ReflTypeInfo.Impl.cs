namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

/// ReflTypeInfo 的函數實現。
public partial class ReflTypeInfo{
	/// 反射建立元資料所需的成員種類（保持與 ITypeInfoSrc / 建構子宣告一致）。
	internal const DynamicallyAccessedMemberTypes ReflDam
		= DynamicallyAccessedMemberTypes.Interfaces
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicFields
		| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

	/// 對一個型別建立元資料：分類、收集成員、找鍵值型別、建無參實例委託。
	public ReflTypeInfo(
		[DynamicallyAccessedMembers(ReflDam)] Type Type
	){
		_type = Type;
		_kind = ComputeKind(Type);
		// 反射收集的順序是「屬性命題在前、字段在後」，統一排序器基類在前把它整成契約序。
		_members = TypeInfoSorter.Sort(Type, CollectMembers(Type));
		_elemType = FindElemType(Type);
		_keyType = FindKeyType(Type);
		var Mk = TryBuildMkInst(Type);
		_mkInstFn = Mk;
		_canMkInst = Mk is not null;
	}

	/// 無參構造：值型別取 default(T)，類型別取無參構造函數；都沒有拋 NotSupportedException。
	public override obj? MkInst(){
		if(_mkInstFn is null){
			throw new NotSupportedException(
				$"型別 {_type.FullName} 沒有可用的無參構造函數（接口/抽象類/無無參構造函數），無法建立實例。"
			);
		}
		return _mkInstFn();
	}

	/// 型別分類：先剝 Nullable，再按 字典→標量→集合→物件 的優先序判定。
	private static ETypeKind ComputeKind([DynamicallyAccessedMembers(ReflDam)] Type T){
		var U = Nullable.GetUnderlyingType(T);
		if(U is not null){
			return ComputeKind(U);
		}
		if(IsDictionary(T)){
			return ETypeKind.Dictionary;
		}
		if(T.IsEnum || T.IsPrimitive || T == typeof(str) || T == typeof(decimal)
			|| T == typeof(DateTime) || T == typeof(DateTimeOffset) || T == typeof(TimeSpan)
			|| T == typeof(Guid) || T == typeof(DateOnly) || T == typeof(TimeOnly))
		{
			return ETypeKind.Scalar;
		}
		if(typeof(System.Collections.IEnumerable).IsAssignableFrom(T)){
			return ETypeKind.Enumerable;
		}
		return ETypeKind.Object;
	}

	/// 是否字典：實現了非泛型 IDictionary，或（直接是/實現了）IDictionary&lt;,&gt;。
	private static bool IsDictionary([DynamicallyAccessedMembers(ReflDam)] Type T){
		if(typeof(System.Collections.IDictionary).IsAssignableFrom(T)){
			return true;
		}
		return FindGenericIface(T, typeof(IDictionary<,>)) is not null;
	}

	/// 集合元素型別：數組取 GetElementType；字典取值型別（與 JsonTypeInfo.ElementType
	/// 一致，STJ 對字典的元素型別就是值型別）；其餘取 IEnumerable&lt;T&gt; 的泛型實參。
	private static Type? FindElemType([DynamicallyAccessedMembers(ReflDam)] Type T){
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

	/// 字典鍵型別：IDictionary&lt;K,V&gt; 的第一個泛型實參。
	private static Type? FindKeyType([DynamicallyAccessedMembers(ReflDam)] Type T){
		var I = FindGenericIface(T, typeof(IDictionary<,>));
		return I?.GetGenericArguments()[0];
	}

	/// 找 T 上實現了「泛型定義為 Def」的最近介面（T 本身是該介面也認）。
	private static Type? FindGenericIface(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type T,
		Type Def
	){
		if(T.IsGenericType && T.GetGenericTypeDefinition() == Def){
			return T;
		}
		return T.GetInterfaces().FirstOrDefault(
			x => x.IsGenericType && x.GetGenericTypeDefinition() == Def
		);
	}

	/// 收集成員：公開實例屬性（排除索引器）+ 公開實例字段，屬性段在前、字段段在後；
	/// 段內順序 = 元數據表序（近似源碼聲明序）。Order = 收集序。
	/// 類型層次的合併由 TypeInfoSorter 統一。
	private static IReadOnlyList<IMemberInfo> CollectMembers(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
		)] Type T
	){
		var R = new List<IMemberInfo>();
		var Order = 0;
		foreach(var Prop in T.GetProperties(BindingFlags.Instance | BindingFlags.Public)){
			if(Prop.GetIndexParameters().Length > 0){
				continue;
			}
			R.Add(new ReflMemberInfo(Prop, Order));
			Order++;
		}
		foreach(var Fld in T.GetFields(BindingFlags.Instance | BindingFlags.Public)){
			R.Add(new ReflMemberInfo(Fld, Order));
			Order++;
		}
		return R;
	}

	/// 建立無參實例委託。值型別取默認值；類型別解析無參構造函數。
	/// JIT 下用表達式樹一次編譯成委託（靜態引用構造函數，剪裁友好）；
	/// NativeAOT 不支持動態編譯（Compile 拋 PlatformNotSupportedException），
	/// 退回 Activator.CreateInstance——構造函數元數據已由 ReflDam 保證保留。
	/// 無構造函數/抽象/接口返回 null。
	private static Func<obj?>? TryBuildMkInst(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
			| DynamicallyAccessedMemberTypes.Interfaces
		)] Type T
	){
		if(T.IsAbstract || T.IsInterface){
			return null;
		}
		if(T.IsValueType){
			// 值型別：CreateInstance 直接產生 boxed 默認值，無需構造函數元數據。
			return () => Activator.CreateInstance(T);
		}
		if(T.GetConstructor(Type.EmptyTypes) is null){
			return null;
		}
		try{
			var Ctor = T.GetConstructor(Type.EmptyTypes)!;
			Expression Body = Expression.New(Ctor);
			var Boxed = Expression.Convert(Body, typeof(obj));
			return Expression.Lambda<Func<obj?>>(Boxed).Compile();
		}
		catch(PlatformNotSupportedException){
			// NativeAOT：退回反射創建，每次調用走一次 Activator（MkInst 頻率低，可接受）。
			return () => Activator.CreateInstance(T)!;
		}
	}
}