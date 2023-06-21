using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortVlanSettingData")]
internal interface IEthernetSwitchPortVlanFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    int AccessVlan { get; set; }

    int NativeVlan { get; set; }

    VlanOperationMode OperationMode { get; set; }

    PrivateVlanMode PrivateMode { get; set; }

    int PrimaryVlan { get; set; }

    int SecondaryVlan { get; set; }

    IList<int> GetTrunkVlanList();

    void SetTrunkVlanList(IList<int> trunkList);

    IList<int> GetSecondaryVlanList();

    void SetSecondaryVlanList(IList<int> secondaryVlanList);
}
