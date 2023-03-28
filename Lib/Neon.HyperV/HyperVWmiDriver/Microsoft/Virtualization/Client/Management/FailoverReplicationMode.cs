using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(FailoverReplicationModeConverter))]
internal enum FailoverReplicationMode
{
	None,
	Primary,
	Recovery,
	TestReplica,
	ExtendedReplica
}
