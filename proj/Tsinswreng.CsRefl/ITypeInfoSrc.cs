namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc("""
#Sum[型別元資料的來源。可插拔、可合成。]

#Descr[
現成實現：
+ `ReflTypeInfoSrc`：兼容 AOT 的反射來源，任何型別都能查（元數據保留時）
+ `JsonTypeInfoSrc`：`System.Text.Json` 源生成上下文，只認已註冊型別
+ `TypeInfoReg`：手寫註冊表（把手工元資料塞進去）
+ `MergedTypeInfoSrc`：多來源按優先級合成

「來源」是行為不是資料：
本接口只有查詢，沒有可寫屬性；
想手動註冊請用 `ITypeInfoReg`，想合成請用 `MergedTypeInfoSrc`。
]
""")]
public interface ITypeInfoSrc{
	[Doc("""
#Sum[取指定型別的元資料；未知返回 false。]

#Params([[要查的型別], [取到的元資料；未知時為 null]])

#Descr[
DAM 註解：反射來源需要被查型別保留
接口/公共屬性/公共字段/無參構造函數 的元數據；
JsonTypeInfo 來源不依賴它，但接口統一宣告了這個前置條件。
]
""")]
	bool TryGetInfo(
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.Interfaces
			| DynamicallyAccessedMemberTypes.PublicProperties
			| DynamicallyAccessedMemberTypes.PublicFields
			| DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
		)] Type Type,
		[NotNullWhen(true)] out ITypeInfo? Info
	);

	[Doc("""
#Sum[列舉本來源所知的所有型別。]

#Descr[
來源不支持列舉時返回 null
（反射來源、JsonTypeInfo 來源都如此）；
調用方不得假設「能列舉」，只把它當加分能力。
]
""")]
	IReadOnlyCollection<Type>? RegisteredTypes{get;}
}