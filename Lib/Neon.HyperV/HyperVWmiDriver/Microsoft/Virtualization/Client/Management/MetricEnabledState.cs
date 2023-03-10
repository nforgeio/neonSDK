namespace Microsoft.Virtualization.Client.Management;

internal enum MetricEnabledState : ushort
{
	Unknown = 0,
	Other = 1,
	Enabled = 2,
	Disabled = 3,
	Reset = 4,
	PartiallyEnabled = 32768
}
