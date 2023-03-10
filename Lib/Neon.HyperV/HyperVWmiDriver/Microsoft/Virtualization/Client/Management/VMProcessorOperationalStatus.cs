using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
[TypeConverter(typeof(VMProcessorOperationalStatusConverter))]
internal enum VMProcessorOperationalStatus
{
	Ok = 2,
	HostResourceProtectionUnknown = 40000,
	HostResourceProtectionEnabled = 40001,
	HostResourceProtectionDisabled = 40002,
	HostResourceProtectionDegradedSuspended = 40003,
	HostResourceProtectionDegradedSerialPipe = 40020,
	HostResourceProtectionDegradedSynthDebug = 40021,
	HostResourceProtectionDegradedLegacyNic = 40022,
	HostResourceProtectionDegradedDvdMedia = 40023,
	HostResourceProtectionDegradedFloppyMedia = 40024
}
