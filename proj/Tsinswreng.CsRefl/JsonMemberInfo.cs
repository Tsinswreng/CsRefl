namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc("""
#Sum[JsonTypeInfo 來源的成員元資料：包一個非泛型 `JsonPropertyInfo`。]

#Descr[
官方事實（2026-09-17 實測，JIT 與 win-x64 NativeAOT 皆同）：

+ 官方 `JsonPropertyInfo` *不繼承* `MemberInfo`——
	它是獨立抽象類，官方這兩個體系沒有共同基類。
	故門面只把官方成員對象交給 `Json` 出口，`Member` 出口留 null。
+ 官方提供 `AttributeProvider`，且源生成下*確實能取到特性*
	（實測 `Level` 上的 `MyDemoAttr` 取回 1 個），所以在 AOT 下同樣可用。
	這一點推翻了本包早期「源生成下特性恒空、統一不支持」的判斷，已改為照官方取。
+ 官方不暴露 `IsProperty`，故無法分辨源生成收進來的字段（`[JsonInclude]`）；
	成員種類只能一律報 `Property`——
	這是兩套來源記錄在案的能力差別。
]

#Descr[
建構子實現見 `JsonMemberInfo.Impl.cs`。
]
""")]
public partial class JsonMemberInfo:MemberInfoBase{
	[Doc("""
#Sum[包一個 `JsonPropertyInfo`。]

#Params([[要包的官方 JSON 成員元資料]])
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Prop);
}