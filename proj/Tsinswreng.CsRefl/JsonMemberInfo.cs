namespace Tsinswreng.CsRefl;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;

[Doc($"""
#Sum[Json 側的成員配接器：把官方 {nameof(JsonPropertyInfo)} 接上 {nameof(IMemberInfo)}。]

#Descr[
源生成給出的成員是官方 {nameof(JsonPropertyInfo)}，本類把它統一成門面的 {nameof(IMemberInfo)}。
]
""")]
public partial class JsonMemberInfo:IMemberInfo{
	[Doc($"""
#Sum[官方成員物件本體（進階用途）。]

#Descr[
構造期賦值，之後不再改；要直接使官方源生成 API，用它。
]
""")]
	public readonly JsonPropertyInfo _Raw;//TswgNote 違反命名規範, 而且爲甚麼不用public?
	// 已按此改：命名改 _Raw（public ＋ 下劃線 ＋ 大駝峯）；改 public，不再 private。

	[Doc($"""
#Sum[用官方成員物件建配接器。]

#Params([[Json, 官方成員物件；不允許 null]])
""")]
	public partial JsonMemberInfo(JsonPropertyInfo Json);

	[Doc($"""
#Sum[成員名，取自官方成員物件。]

#See[{nameof(IMemberInfo.Name)}]
""")]
	public str Name{
		get{
			return _Raw.Name;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[成員型別，取自官方 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.PropertyType)}。]

#See[{nameof(IMemberInfo.PropertyType)}]
""")]
	public Type PropertyType{
		get{
			return _Raw.PropertyType;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[成員宣告所在的型別，取自官方成員物件。]

#See[{nameof(IMemberInfo.OwnerType)}]
""")]
	public Type OwnerType{
		get{
			return _Raw.DeclaringType;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[看官方的讀取委託 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Get)} 是否非 null。]

#See[{nameof(IMemberInfo.CanRead)}]
""")]
	public bool CanRead{
		get{
			return _Raw.Get is not null;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[看官方的寫入委託 {nameof(JsonPropertyInfo)}.{nameof(JsonPropertyInfo.Set)} 是否非 null。]

#See[{nameof(IMemberInfo.CanWrite)}]
""")]
	public bool CanWrite{
		get{
			return _Raw.Set is not null;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	[Doc($"""
#Sum[取官方成員物件自帶的特性提供者。]

#See[{nameof(IMemberInfo.AttributeProvider)}]
""")]
	public ICustomAttributeProvider? AttributeProvider{
		get{
			return _Raw.AttributeProvider;
		}
		set{
			// 佔位：本輪只加形狀，實現待寫（本配接器是現讀官方物件，覆蓋語義待定）。
			throw new NotImplementedException();
		}
	}

	public partial bool TryGet(obj? O, out obj? V);
	public partial bool TrySet(obj? O, obj? V);
}




