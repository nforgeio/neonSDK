namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_CDROMDrive")]
internal interface IPhysicalCDRomDrive : IVirtualizationManagementObject
{
	[Key]
	string DeviceId { get; }

	string Drive { get; }

	string PnpDeviceId { get; }
}
