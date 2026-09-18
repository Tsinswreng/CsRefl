namespace Tsinswreng.CsRefl;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[反射來源：對任意型別用反射建立 `ITypeInfo`，並按型別緩存。]

#Descr[
AOT 下可用——前提是查詢目標的成員元數據已被保留
（見 `ReflMemberInfo` 的說明）。

`RegisteredTypes` 返回 null：
反射來源能查任意型別，無法也無需列舉。

`TryGetInfo` 實現見 `ReflTypeInfoSrc.Impl.cs`。
]
""")]
public partial class ReflTypeInfoSrc:ITypeInfoSrc{
	[Doc("""
#Sum[型別 → 元資料緩存。]

#Descr[
同一型別只建一次（元資料構建有反射代價）。
]
""")]
	private readonly ConcurrentDictionary<Type, ITypeInfo> _cache = new();

	[Doc("""
#Sum[默認單例：不經 DI 也能直接使用的反射來源。]
""")]
	public static ReflTypeInfoSrc Inst{
		get;
	} = new();

	[Doc("""
#Sum[不支持列舉（反射來源能查任意型別）。]

#See[{nameof(ITypeInfoSrc.RegisteredTypes)}]
""")]
	public IReadOnlyCollection<Type>? RegisteredTypes{
		get{
			return null;
		}
	}

	[Doc("""
#Sum[取任意型別的元資料；反射來源總是「可知」。]

#See[{nameof(ITypeInfoSrc.TryGetInfo)}]
""")]
	public partial bool TryGetInfo(
		[DynamicallyAccessedMembers(ReflTypeInfo.ReflDam)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);
}