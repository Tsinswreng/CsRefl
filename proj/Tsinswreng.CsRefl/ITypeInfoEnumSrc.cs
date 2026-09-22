namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($$"""
#Sum[手上有一份「型別到元資料」表的來源。]

#Descr[
本接口在 {{nameof(ITypeInfoSrc)}} 的查詢能力之上，多要求一樣東西：
一條來源能交出它手上的那份表，也就是它知道自己有哪些型別。

為甚麼要另開一個接口：不是每條來源都有這份表。

+ {{nameof(ReflTypeInfoSrc)}} 對任意型別都能當場用反射造元資料，它沒有一份型別清單，也不需要有；
+ {{nameof(JsonTypeInfoSrc)}} 的型別清單在官方源生成上下文裏，官方不提供取出的途徑。

這兩條來源只實作 {{nameof(ITypeInfoSrc)}}。它們答不出「我有哪些型別」，
故本接口不長在她們身上，免得讀者以為「這條來源一條型別都沒有」。

有表的來源才實作本接口：

+ {{nameof(TypeInfoReg)}}：表就是它登記過的內容；
+ {{nameof(MergedTypeInfoSrc)}}：自己沒有表，但成員來源都拿得出表時，它能現算一份並集。

要列舉型別的人先判斷一次：

```csharp
if(Src is {{nameof(ITypeInfoEnumSrc)}} Enum && Enum.{{nameof(RegisteredTypes)}} is not null){
	// 走全庫掃描
}
```
]
""")]
public interface ITypeInfoEnumSrc:ITypeInfoSrc{
	[Doc($$"""
#Sum[本來源手上的「型別 → 元資料」表。]

#Descr[
鍵是型別，值是該型別的元資料（與 {{nameof(TryGetInfo)}} 交出的是同一批物件）。
要型別清單就取這張表的鍵集合。

{{nameof(TypeInfoReg)}} 交出的是它登記過的內容；
{{nameof(MergedTypeInfoSrc)}} 交出的是成員來源的並集，成員來源之中只要有一條拿不出表，
它就交不出並集，此時本屬性的值是 null。

賦值＝把這條來源的表整張換掉：不合併、不拷貝，給什麼就存什麼。
拿到的那份字典歸誰、之後要不要改、改了算不算數，由調用方自己負責。

調用方這樣寫：

```csharp
var Reg = new {{nameof(TypeInfoReg)}}();
Reg.{{nameof(TypeInfoReg.Add)}}(typeof({{nameof(ITypeInfo)}}), Info);

var Map = Reg.{{nameof(RegisteredTypes)}}!;
Map[typeof({{nameof(ITypeInfo)}})];                                                       // 剛才登記進去的那個 Info
Map.{{nameof(IDictionary<Type, ITypeInfo>.ContainsKey)}}(typeof({{nameof(ITypeInfoSrc)}}));   // false：沒登記過
```
]
""")]
	//TswgNote 爲甚麼用List? 怎麼到處都在用list?
	// 已按此改：RegisteredTypes 由 IReadOnlyCollection<Type> 改為 IDictionary<Type, ITypeInfo>。
	// 理由：鍵與值是同一張表的兩半，只交鍵會讓「列舉到的型別」與「隨後查到的元資料」不是同一時刻。
	IDictionary<Type, ITypeInfo>? RegisteredTypes{get;set;}
}
