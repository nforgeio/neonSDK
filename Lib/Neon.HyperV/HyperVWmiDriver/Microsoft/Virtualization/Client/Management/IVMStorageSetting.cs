namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_StorageSettingData")]
internal interface IVMStorageSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	ushort ThreadCountPerChannel { get; set; }

	ushort VirtualProcessorsPerChannel { get; set; }

	bool DisableInterruptBatching { get; set; }
}
