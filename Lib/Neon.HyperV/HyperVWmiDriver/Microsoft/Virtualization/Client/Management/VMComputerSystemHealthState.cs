using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
internal enum VMComputerSystemHealthState
{
	Unknown = 0,
	Ok = 5,
	MajorFailure = 20,
	CriticalFailure = 25
}
