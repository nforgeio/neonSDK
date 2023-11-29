namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemManagementServiceSettingData")]
internal interface IVMServiceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string DefaultExternalDataRoot { get; set; }

    string DefaultVirtualHardDiskPath { get; set; }

    string MinimumMacAddress { get; set; }

    string MaximumMacAddress { get; set; }

    string MinimumWorldWidePortName { get; set; }

    string MaximumWorldWidePortName { get; set; }

    string AssignedWorldWideNodeName { get; set; }

    bool NumaSpanningEnabled { get; set; }

    bool NumaSpanningSupported { get; }

    bool EnhancedSessionModeEnabled { get; set; }

    bool EnhancedSessionModeSupported { get; }

    bool HypervisorRootSchedulerEnabled { get; }
}
