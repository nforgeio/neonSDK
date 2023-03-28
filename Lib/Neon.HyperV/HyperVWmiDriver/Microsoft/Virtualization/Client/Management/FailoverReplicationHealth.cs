using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(FailoverReplicationHealthConverter))]
internal enum FailoverReplicationHealth
{
	NotApplicable,
	Normal,
	Warning,
	Critical
}
