using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMNetworkingFeatureBase : VirtualizationObject, IAddable, IUpdatable, IRemovable
{
    protected readonly IEthernetSwitchFeatureService m_FeatureService;

    internal readonly IEthernetFeature m_FeatureSetting;

    public bool IsTemplate { get; private set; }

    protected abstract string DescriptionForPut { get; }

    protected abstract string DescriptionForRemove { get; }

    internal VMNetworkingFeatureBase(IEthernetFeature featureSetting, IEthernetSwitchFeatureService featureService, bool isTemplate)
        : base(featureSetting)
    {
        m_FeatureService = featureService;
        m_FeatureSetting = featureSetting;
        IsTemplate = isTemplate;
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformOperation(() => m_FeatureSetting.BeginModifySingleFeature(m_FeatureService), m_FeatureSetting.EndModifySingleFeature, DescriptionForPut, this);
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformOperation(() => m_FeatureService.BeginRemoveFeatures(new IEthernetFeature[1] { m_FeatureSetting }), m_FeatureService.EndRemoveFeatures, DescriptionForRemove, this);
        ResetParentFeatureCache();
    }

    protected abstract void ResetParentFeatureCache();

    internal static IEthernetFeature GetDefaultFeatureSettingInstance(Server server, EthernetFeatureType featureType)
    {
        return ObjectLocator.GetHostComputerSystem(server).GetFeatureCapabilities(featureType, Capabilities.DefaultCapability);
    }
}
