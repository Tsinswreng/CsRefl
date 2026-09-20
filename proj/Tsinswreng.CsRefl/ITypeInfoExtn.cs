namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[門面的順手寫法：把「按名查成員 + 讀寫該成員」兩步合成一次調用。]

#Descr[
分工：按名查本身在 {nameof(ITypeInfo)}.{nameof(ITypeInfo.TryGetMember)} 上
（它要按實例緩存、必須 O(1)），成員自身的讀寫判據在 {nameof(MemberExtn)} 上；
本類只補中間那一格——手上有型別元資料、又只想要某個名字的值時，不必自己寫兩步。

實測：不必先 {nameof(ITypeInfo.TryGetMember)} 再 {nameof(MemberExtn.TryGet)} 兩步，
直接 `Info.{nameof(TryGet)}(User, "Age", out var V)` 一次拿到值，
當 `User.Age` 是 26 時 `V` 是 boxed 的 `i32` 26。

參數序按範圍由大到小：實例 → 成員名 → 收值的那個出參。
型別由擴展方法的受體（那個 {nameof(ITypeInfo)} 實例）本身承擔，故它已經在最前面。

按名讀寫的判據是「成員自身能力」，不是「在不在某張鍵表裏」：
只讀成員讀得到、寫不進；只寫成員寫得進、讀不到。

實現見 `ITypeInfoExtn.Impl.cs`。
]
""")]
public static partial class ITypeInfoExtn{
	[Doc($$"""
#Sum[按名讀值。]

#Params([[z, 型別元資料], [O, 實例], [Name, 成員名], [V, 讀出的值；失敗時為 default]])

#Rtn[成員不存在、不可讀、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo(typeof(PoUser));
var User = new PoUser{ Age = 26 };

Info.TryGet(User, nameof(PoUser.Age), out var V);
// true；V 是 boxed 的 i32 26。

Info.TryGet(User, nameof(PoUser.Secret), out var S);
// true；Secret 是只讀成員，讀得到——V 是 "s"。

Info.TryGet(User, nameof(PoUser.Token), out _);   // false：只寫成員讀不到
Info.TryGet(User, "NoSuch", out _);               // false：成員不存在
Info.TryGet(null, nameof(PoUser.Age), out _);     // false：實例是 null
```

失敗一律用 false 表示（不拋），故適合按外部欄位名取值這類「能取就取」的場景。
]
""")]
	public static partial bool TryGet(this ITypeInfo z, obj? O, str Name, out obj? V);

	[Doc($$"""
#Sum[按名讀值（泛型版：實例的靜態型別就是 `T`）。]

#TParams[實例的靜態型別]

#Params([[z, 型別元資料], [O, 實例], [Name, 成員名], [V, 讀出的值；失敗時為 default]])

#Rtn[成員不存在、不可讀、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();
var User = new PoUser{ Age = 26 };

Info.TryGet<PoUser>(User, nameof(PoUser.Age), out var V);
// true；V 是 boxed 的 i32 26。

Info.TryGet<PoUser>(User, nameof(PoUser.Token), out _);   // false：只寫成員讀不到
```

與非泛型版的差別只有一處：實例以 `T` 傳入，型別不符在編譯期就被擋住。
]
""")]
	public static partial bool TryGet<T>(this ITypeInfo z, T O, str Name, out obj? V);

	[Doc($$"""
#Sum[按名寫值。]

#Params([[z, 型別元資料], [O, 實例], [Name, 成員名], [V, 要寫入的值]])

#Rtn[成員不存在、不可寫、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo(typeof(PoUser));
var User = new PoUser{ Age = 26 };

Info.TrySet(User, nameof(PoUser.Age), 31);
// true；之後 User.Age 是 31。

Info.TrySet(User, nameof(PoUser.Secret), "x");
// false：Secret 是只讀成員，且 User.Secret 仍是 "s"（不動實例）。

Info.TrySet(User, nameof(PoUser.Age), "不是數字");
// 拋（不是返回 false）：值型別不符是調用方的 bug。
```

參數序同 {{nameof(TryGet)}}：實例 → 成員名 → 值。
]
""")]
	public static partial bool TrySet(this ITypeInfo z, obj? O, str Name, obj? V);

	[Doc($$"""
#Sum[按名寫值（泛型版：實例的靜態型別就是 `T`）。]

#TParams[實例的靜態型別]

#Params([[z, 型別元資料], [O, 實例], [Name, 成員名], [V, 要寫入的值]])

#Rtn[成員不存在、不可寫、實例為 null 或型別不符時返回 false]

#Descr[
調用方這樣寫：

```csharp
var Info = Src.GetInfo<PoUser>();
var User = new PoUser{ Age = 26 };

Info.TrySet<PoUser>(User, nameof(PoUser.Age), 31);
// true；之後 User.Age 是 31。

Info.TrySet<PoUser>(User, nameof(PoUser.Secret), "x");
// false：只讀成員，且 User.Secret 仍是 "s"。
```

值型別不符照常拋（與非泛型版同一條規矩）。
]
""")]
	public static partial bool TrySet<T>(this ITypeInfo z, T O, str Name, obj? V);


}

