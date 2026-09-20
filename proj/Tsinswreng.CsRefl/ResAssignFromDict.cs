namespace Tsinswreng.CsRefl;

using Tsinswreng.CsCore;

[Doc($"""
#Sum[AssignFromDict 的返回值；現在是空殼，先佔位、日後再填。]

#Descr[
先留空：日後要在這裏放「哪些鍵被跳過、為甚麼跳過」這類資訊時，
不必再改 AssignFromDict 的簽名（那會動到所有調用方）。

實測：AssignFromDict 現在每次返回一個新實例，裏面沒有任何成員。
]
""")]
public partial class ResAssignFromDict{
}
