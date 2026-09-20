namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[門面的順手寫法：把「按名查成員 + 讀寫該成員」兩步合成一次調用。]

#Descr[
分工：按名查本身在 {nameof(ITypeInfo)}.{nameof(ITypeInfo.TryGetMember)} 上
（它要按實例緩存、必須 O(1)），成員自身的讀寫判據在 {nameof(MemberExtn)} 上；
本類只補中間那一格——手上有型別元資料、又只想要某個名字的值時，不必自己寫兩步。

實測：不必先 {nameof(ITypeInfo.TryGetMember)} 再 {nameof(MemberExtn.TryGet)} 兩步，
直接 `Info.{nameof(TryGet)}("Age", User, out var V)` 一次拿到值，
當 `User.Age` 是 26 時 `V` 是 boxed 的 `i32` 26。

按名讀寫的判據是「成員自身能力」，不是「在不在某張鍵表裏」：
只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。

實現見 `ITypeInfoExtn.Impl.cs`。
]
""")]
public static partial class ITypeInfoExtn{
	[Doc($"""
#Sum[按名讀值。]

#Params([[z, 型別元資料], [Name, 成員名], [O, 實例], [V, 讀出的值；失敗時為 default]])

#Rtn[成員不存在、不可讀、實例為 null 或型別不符時返回 false]

#Descr[
實測（`PoUser`，`Age = 30`、`Secret = "s"`）：

+ `{nameof(TryGet)}(Info, "Age", User, out var V)` 返回 true 且 `V` 是 boxed 的 `i32` 30；
+ `"Secret"`（只讀）返回 true 且 `V` 是 "s"；
+ `"Token"`（只寫）返回 false；`"NoSuch"` 返回 false；傳 null 實例返回 false。

失敗一律用 false 表示（不拋），故適合按外部欄位名取值這類「能取就取」的場景。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, str Name, obj? O, out obj? V);

	[Doc($"""
#Sum[按名寫值。]

#Params([[z, 型別元資料], [Name, 成員名], [O, 實例], [V, 要寫入的值]])

#Rtn[成員不存在、不可寫、實例為 null 或型別不符時返回 false]

#Descr[
值型別不符不返回 false，而是照常拋（那是調用方的 bug，不是「不可寫」）。

實測（`PoUser`）：`{nameof(TrySet)}(Info, "Age", User, 31)` 返回 true 且之後 `User.Age` 是 31；
只讀的 `"Secret"` 返回 false 且不動實例。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, str Name, obj? O, obj? V);
}
