using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchFeatureCapabilities")]
internal interface IEthernetFeatureCapabilities : IVirtualizationManagementObject
{
    string FeatureId { get; }

    IEnumerable<IEthernetFeature> FeatureSettings { get; }
}
