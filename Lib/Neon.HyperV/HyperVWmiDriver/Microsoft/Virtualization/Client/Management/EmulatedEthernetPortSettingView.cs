namespace Microsoft.Virtualization.Client.Management;

internal class EmulatedEthernetPortSettingView : EthernetPortSettingView, IEmulatedEthernetPortSetting, IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.EthernetPortEmulated;
}
