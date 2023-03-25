namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SyntheticDisplayControllerSettingData")]
internal interface IVMSyntheticDisplayControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	ResolutionType ResolutionType { get; set; }

	int HorizontalResolution { get; set; }

	int VerticalResolution { get; set; }
}
