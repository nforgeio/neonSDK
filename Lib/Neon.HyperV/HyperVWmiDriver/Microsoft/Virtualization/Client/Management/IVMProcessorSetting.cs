namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ProcessorSettingData")]
internal interface IVMProcessorSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
    int Reservation { get; set; }

    int VirtualQuantity { get; set; }

    int Weight { get; set; }

    long MaxProcessorsPerNumaNode { get; set; }

    long MaxNumaNodesPerSocket { get; set; }

    int Limit { get; set; }

    bool LimitCpuId { get; set; }

    long? HwThreadsPerCore { get; set; }

    bool LimitProcessorFeatures { get; set; }

    bool EnableHostResourceProtection { get; set; }

    bool ExposeVirtualizationExtensions { get; set; }

    bool EnablePerfmonPmu { get; set; }

    bool EnablePerfmonLbr { get; set; }

    bool EnablePerfmonPebs { get; set; }

    bool EnablePerfmonIpt { get; set; }

    bool EnableLegacyApicMode { get; set; }

    bool AllowACountMCount { get; set; }
}
