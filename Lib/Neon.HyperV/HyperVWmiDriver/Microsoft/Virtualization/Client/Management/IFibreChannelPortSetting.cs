namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SyntheticFcPortSettingData", PrimaryMapping = true)]
internal interface IFibreChannelPortSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	string WorldWideNodeName { get; set; }

	string WorldWidePortName { get; set; }

	string SecondaryWorldWideNodeName { get; set; }

	string SecondaryWorldWidePortName { get; set; }

	IFcPoolAllocationSetting GetConnectionConfiguration();
}
