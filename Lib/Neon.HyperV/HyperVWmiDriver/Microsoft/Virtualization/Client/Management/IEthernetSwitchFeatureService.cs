using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IEthernetSwitchFeatureService : IVirtualizationManagementObject
{
    IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, IEthernetSwitchPortFeature[] features);

    IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, string[] featureEmbeddedInstances);

    IEnumerable<IEthernetSwitchPortFeature> EndAddPortFeatures(IVMTask task);

    IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, IEthernetSwitchFeature[] features);

    IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, string[] featureEmbeddedInstances);

    IEnumerable<IEthernetSwitchFeature> EndAddSwitchFeatures(IVMTask task);

    IVMTask BeginModifyFeatures(IEthernetFeature[] features);

    IVMTask BeginModifyFeatures(string[] featureEmbeddedInstances);

    void EndModifyFeatures(IVMTask task);

    IVMTask BeginRemoveFeatures(IEthernetFeature[] features);

    void EndRemoveFeatures(IVMTask task);
}
