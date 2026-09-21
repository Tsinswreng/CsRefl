namespace Tsinswreng.CsRefl;

using System.Reflection;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[{nameof(ReflMemberInfo)} 的函數實現（先佔位）。]
""")]
public partial class ReflMemberInfo{
	public partial ReflMemberInfo(MemberInfo Member){
		_Raw = Member;
		throw new NotImplementedException();
	}

	public partial bool TryGet(obj? O, out obj? V){
		throw new NotImplementedException();
	}

	public partial bool TrySet(obj? O, obj? V){
		throw new NotImplementedException();
	}
}




