using System;
using System.Text.RegularExpressions;

namespace Neon.K8s;

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

internal class StringQuotingEmitter : ChainedEventEmitter
{
	private static readonly Regex QuotedRegex = new Regex("^(\\~|null|Null|NULL|true|True|TRUE|false|False|FALSE|y|Y|yes|Yes|YES|on|On|ON|n|N|no|No|NO|off|Off|OFF|-?(0|[0-9]*)(\\.[0-9]*)?([eE][-+]?[0-9]+)?)?$");

	public StringQuotingEmitter(IEventEmitter next)
		: base(next)
	{
	}

	public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
	{
		switch ((eventInfo != null && ((EventInfo)eventInfo).Source.Value != null) ? Type.GetTypeCode(((EventInfo)eventInfo).Source.Type) : TypeCode.Empty)
		{
		case TypeCode.Char:
			if (char.IsDigit((char)((EventInfo)eventInfo).Source.Value))
			{
				eventInfo.Style = (ScalarStyle)3;
			}
			break;
		case TypeCode.String:
		{
			string text = ((EventInfo)eventInfo).Source.Value.ToString();
			if (QuotedRegex.IsMatch(text))
			{
				eventInfo.Style = (ScalarStyle)3;
			}
			else if (text.IndexOf('\n') > -1)
			{
				eventInfo.Style = (ScalarStyle)4;
			}
			break;
		}
		}
		((ChainedEventEmitter)this).Emit(eventInfo, emitter);
	}
}
