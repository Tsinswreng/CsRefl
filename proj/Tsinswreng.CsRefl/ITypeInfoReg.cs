namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[可寫的型別元資料註冊表：在 {nameof(ITypeInfoSrc)} 之上補上手動增刪。]

#Descr[
與舊 Srefl 的「可寫字典」不同：
這裡註冊的內容是 {nameof(ITypeInfo)}（元資料對象），
而不是一張可以被任意替換的字典；
合併與優先級交給 {nameof(MergedTypeInfoSrc)}。

例：某個型別既沒掛 `[JsonSerializable]`、又不適合走反射
（例如它的成員是運行期才知道的），
就可以手工造一份元資料塞進註冊表，讓上層代碼照常查得到。
]
""")]
public interface ITypeInfoReg:ITypeInfoSrc{
	[Doc($"""
#Sum[登記一個型別的元資料。]

#Params([[要登記的型別], [該型別的元資料]])

#Descr[
重複登記同一型別拋 {nameof(InvalidOperationException)}
（防止無意覆蓋；確要替換先 {nameof(Remove)} 再 {nameof(Add)}）。

例：`Reg.{nameof(Add)}(typeof(Xxx), new {nameof(ReflTypeInfo)}(typeof(Xxx)))` 之後
`Reg.{nameof(TryGetInfo)}(typeof(Xxx), out var Info)` 就命中；
再 `Reg.{nameof(Add)}(typeof(Xxx), ...)` 一次會拋異常，
這是有意為之：初始化期重複登記通常是配置寫錯了，靜默覆蓋會讓問題藏到很後面。
]
""")]
	void Add(Type Type, ITypeInfo Info);

	[Doc($"""
#Sum[移除一個型別。]

#Params([[要移除的型別]])

#Rtn[原本不存在返回 false]

#Descr[
例：`Reg.{nameof(Remove)}(typeof(Xxx))` 第一次返回 true，
再調一次返回 false（本來就沒有了），之後 {nameof(TryGetInfo)} 也查不到。

要替換已登記的元資料時就是靠它：先 {nameof(Remove)} 再 {nameof(Add)}。
]
""")]
	bool Remove(Type Type);
}