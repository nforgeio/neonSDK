using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
[TypeConverter(typeof(VMProcessorOperationalStatusConverter))]
internal enum VMProcessorOperationalStatus
{
	Ok = 2,
	HostResourceProtectionFirst = 40000,
	HostResourceProtectionUnknown = 40000,
	HostResourceProtectionEnabled = 40001,
	HostResourceProtectionDisabled = 40002,
	HostResourceProtectionDegradedSuspended = 40003,
	HostResourceProtectionLast = 40004,
	HostResourceProtectionDegradedFirst = 40020,
	HostResourceProtectionDegradedSerialPipe = 40020,
	HostResourceProtectionDegradedSynthDebug = 40021,
	HostResourceProtectionDegradedLegacyNic = 40022,
	HostResourceProtectionDegradedDvdMedia = 40023,
	HostResourceProtectionDegradedFloppyMedia = 40024,
	HostResourceProtectionDegradedLast = 40025
}
