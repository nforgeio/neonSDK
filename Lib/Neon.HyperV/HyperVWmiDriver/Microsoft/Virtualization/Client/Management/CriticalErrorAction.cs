using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<CriticalErrorAction>))]
internal enum CriticalErrorAction
{
	None,
	Pause
}
