using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MetricServiceSettingData")]
internal interface IMetricServiceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    TimeSpan FlushInterval { get; set; }
}
