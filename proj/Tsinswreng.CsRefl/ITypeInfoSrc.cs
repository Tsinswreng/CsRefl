namespace Tsinswreng.CsRefl;

using System.Diagnostics.CodeAnalysis;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[型別元資料的來源。可插拔、可合成。]

#Descr[
現成實現各自覆蓋一種場景：
+ {nameof(ReflTypeInfoSrc)}：兼容 AOT 的反射來源，任何型別都能查（元數據保留時）
+ {nameof(JsonTypeInfoSrc)}：`System.Text.Json` 源生成上下文，只認已註冊型別
+ {nameof(TypeInfoReg)}：手寫註冊表（把手工元資料塞進去）
+ {nameof(MergedTypeInfoSrc)}：多來源按優先級合成

實測：AOT 主路徑上掛 {nameof(MergedTypeInfoSrc)}，
把 {nameof(JsonTypeInfoSrc)} 放第一位（讀寫走源生成的委託，零反射）、
{nameof(ReflTypeInfoSrc)} 放第二位兜底，
於是 `typeof(PoUser)` 由 Json 源接住（回傳實例與 Json 源單獨查到的 `{nameof(ReferenceEquals)}` 為 true），
而沒掛 `[JsonSerializable]` 的 `typeof(PoNoCtor)` 落到反射源、照樣查得到。
]

#Descr[
「來源」是行為不是資料：
本接口只有查詢，沒有可寫屬性；
想手動註冊請用 {nameof(ITypeInfoReg)}，想合成請用 {nameof(MergedTypeInfoSrc)}。
]
""")]
public interface ITypeInfoSrc{
	[Doc($"""
#Sum[取指定型別的元資料；未知返回 false。]

#Params([[Type, 要查的型別], [Info, 取到的元資料；未知時為 null]])

#Descr[
DAM 註解：反射來源需要被查型別保留
接口、公共屬性、公共字段、無參構造函數 的元數據；
JsonTypeInfo 來源不依賴它，但接口統一宣告了這個前置條件。

實測：從 {nameof(JsonTypeInfoSrc)} 查 `typeof(PoUser)` 返回 true、`Info` 非 null；
查沒掛過的 `typeof(PoNoCtor)` 返回 false、`Info` 為 null
（未註冊的型別會被記進負面緩存，故重複查不會反復走 resolver 鏈）。
從 {nameof(ReflTypeInfoSrc)} 查兩者都返回 true，這正是「兜底」的意義。

要「查不到就拋異常」的便利版本用 {nameof(ITypeInfoSrcExtn)}.{nameof(ITypeInfoSrcExtn.GetMember)}。
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

	[Doc($"""
#Sum[列舉本來源所知的所有型別。]

#Descr[
來源不支持列舉時返回 null
（{nameof(ReflTypeInfoSrc)}、{nameof(JsonTypeInfoSrc)} 都如此）；
調用方不得假設「能列舉」，只把它當加分能力。

實測：{nameof(TypeInfoReg)} 登記 `typeof(PoUser)` 與 `typeof(PoColor)` 後返回 2 個 `{nameof(Type)}`；
{nameof(JsonTypeInfoSrc)} 返回 null，因為官方上下文不暴露已註冊清單
（{nameof(ReflTypeInfoSrc)} 同樣返回 null）；
{nameof(MergedTypeInfoSrc)} 只在全部成員來源都能列舉時才給出並集，否則整個返回 null。

拼來源時可以先查這個屬性，返回 null 就說明這條鏈不能當「型別全集」用，
例如不能用它來做全庫掃描。
]
""")]
	IReadOnlyCollection<Type>? RegisteredTypes{get;}
}