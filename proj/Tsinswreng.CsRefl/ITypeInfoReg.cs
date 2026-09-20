namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[可寫的型別元資料註冊表：在 {nameof(ITypeInfoSrc)} 之上補上手動增刪。]

#Descr[
與舊 Srefl 的「可寫字典」不同：
這裡註冊的內容是 {nameof(ITypeInfo)}（元資料對象），
而不是一張可以被任意替換的字典；
合併與優先級交給 {nameof(MergedTypeInfoSrc)}。

實測：把一個手工造的 `PoColor` 元資料塞進註冊表，
（例如它的成員是運行期才知道的），
就可以手工造一份元資料塞進註冊表，讓上層代碼照常查得到。
]
""")]
public interface ITypeInfoReg:ITypeInfoSrc{
	[Doc($$"""
#Sum[登記一個型別的元資料。]

#Params([[Type, 要登記的型別], [Info, 該型別的元資料]])

#Descr[
調用方這樣寫：

```csharp
var Reg = new TypeInfoReg();
var Info = new ReflTypeInfo(typeof(PoUser));

Reg.Add(typeof(PoUser), Info);
Reg.TryGetInfo(typeof(PoUser), out var Got);
// true；Got 就是剛才塞進去的那個實例（ReferenceEquals 為 true）。

Reg.Add(typeof(PoUser), Info);
// 拋 InvalidOperationException：重複登記是有意擋住的，
// 因為初始化期重複登記通常是配置寫錯，靜默覆蓋會讓問題藏到很後面。
```

要替換已登記的元資料就先 {{nameof(Remove)}} 再 {{nameof(Add)}}。
]
""")]
	void Add(Type Type, ITypeInfo Info);

	[Doc($$"""
#Sum[移除一個型別。]

#Params([[Type, 要移除的型別]])

#Rtn[原本不存在返回 false]

#Descr[
調用方這樣寫：

```csharp
Reg.Remove(typeof(PoUser));   // true：原本登記過，移掉
Reg.Remove(typeof(PoUser));   // false：本來就沒有了
Reg.TryGetInfo(typeof(PoUser), out _);   // false：移除之後查不到
```

要替換已登記的元資料就是靠它：先 {{nameof(Remove)}} 再 {{nameof(Add)}}。
]
""")]
	bool Remove(Type Type);
}