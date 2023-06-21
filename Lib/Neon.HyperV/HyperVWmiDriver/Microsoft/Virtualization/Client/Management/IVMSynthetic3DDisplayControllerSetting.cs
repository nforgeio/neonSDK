namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Synthetic3DDisplayControllerSettingData")]
internal interface IVMSynthetic3DDisplayControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    int MaximumScreenResolution { get; set; }

    int MaximumMonitors { get; set; }

    ulong VRAMSizeBytes { get; set; }
}
