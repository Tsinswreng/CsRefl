namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc("""
#Sum[反射來源的成員元資料：包一個 `PropertyInfo` 或 `FieldInfo`。]

#Descr[
只收錄公開、實例、非索引器成員
（與 JsonTypeInfo 來源的口徑一致；非公開成員按已定決策不進門面）。
]

#Descr[
AOT 前提：成員元數據必須被保留
（例如該型別同時掛了 `[JsonSerializable]`，或經 ILLink 模式匹配保留了成員），
否則運行期會因缺元數據拋錯——
這是「兼容 AOT 的反射」的固有前提，由調用方負責，本類不隱藏它。
]

#Descr[
建構子實現見 `ReflMemberInfo.Impl.cs`。
]
""")]
public partial class ReflMemberInfo:MemberInfoBase{
	[Doc("""
#Sum[包一個公開實例屬性。]

#Params([[要包的屬性]])
""")]
	internal partial ReflMemberInfo(PropertyInfo Prop);

	[Doc("""
#Sum[包一個公開實例字段。]

#Params([[要包的字段]])
""")]
	internal partial ReflMemberInfo(FieldInfo Fld);
}