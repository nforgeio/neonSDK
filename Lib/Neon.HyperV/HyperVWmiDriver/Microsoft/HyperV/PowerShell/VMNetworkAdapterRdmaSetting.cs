using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterRdmaSetting : VMNetworkAdapterFeatureBase
{
    public uint RdmaWeight
    {
        get
        {
            return NumberConverter.Int32ToUInt32(((IEthernetSwitchPortRdmaFeature)m_FeatureSetting).RdmaOffloadWeight);
        }
        internal set
        {
            ((IEthernetSwitchPortRdmaFeature)m_FeatureSetting).RdmaOffloadWeight = NumberConverter.UInt32ToInt32(value);
        }
    }

    internal VMNetworkAdapterRdmaSetting(IEthernetSwitchPortRdmaFeature offloadSetting, VMNetworkAdapterBase parentAdapter)
        : base(offloadSetting, parentAdapter, isTemplate: false)
    {
    }

    private VMNetworkAdapterRdmaSetting(VMNetworkAdapterBase parentAdapter)
        : base(parentAdapter, EthernetFeatureType.Rdma)
    {
    }

    internal static VMNetworkAdapterRdmaSetting CreateTemplateOffloadSetting(VMNetworkAdapterBase parentAdapter)
    {
        return new VMNetworkAdapterRdmaSetting(parentAdapter);
    }
}
