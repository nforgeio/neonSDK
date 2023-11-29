using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchExtensionPortFeature : VMSwitchExtensionCustomFeature
{
    internal VMNetworkAdapterBase ParentAdapter { get; private set; }

    protected override string DescriptionForPut => TaskDescriptions.SetVMNetworkAdapterFeature;

    protected override string DescriptionForRemove => TaskDescriptions.RemoveVMNetworkAdapterFeature;

    internal VMSwitchExtensionPortFeature(IEthernetSwitchPortFeature featureSetting, VMNetworkAdapterBase parentAdapter)
        : base(featureSetting, parentAdapter?.FeatureService)
    {
        ParentAdapter = parentAdapter;
    }

    internal static IEnumerable<VMSwitchExtensionPortFeature> GetTemplatePortFeatures(Server server)
    {
        return from feature in ObjectLocator.GetHostComputerSystem(server).GetAllFeatures(Capabilities.DefaultCapability).OfType<IEthernetSwitchPortFeature>()
            select new VMSwitchExtensionPortFeature(feature, null);
    }

    protected override void ResetParentFeatureCache()
    {
        ParentAdapter.InvalidateFeatureCache();
    }
}
