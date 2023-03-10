namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ShutdownComponentSettingData")]
internal interface IVMShutdownComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
