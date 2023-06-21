using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMSwitchExtensionCustomFeature : VirtualizationObject, IUpdatable, IRemovable
{
    protected readonly IEthernetFeature m_FeatureSetting;

    private readonly ICimInstance m_SettingDataObject;

    protected readonly IEthernetSwitchFeatureService m_FeatureService;

    public Guid? Id { get; private set; }

    public string ExtensionId { get; private set; }

    public string ExtensionName { get; private set; }

    public string Name { get; private set; }

    public CimInstance SettingData
    {
        get
        {
            if (m_SettingDataObject != null)
            {
                return m_SettingDataObject.Instance;
            }
            return null;
        }
    }

    protected abstract string DescriptionForPut { get; }

    protected abstract string DescriptionForRemove { get; }

    internal VMSwitchExtensionCustomFeature(IEthernetFeature featureSetting, IEthernetSwitchFeatureService featureService)
        : base(featureSetting)
    {
        m_FeatureSetting = featureSetting;
        Name = featureSetting.Name;
        ExtensionId = featureSetting.ExtensionId;
        if (Guid.TryParse(featureSetting.FeatureId, out var result))
        {
            Id = result;
        }
        IInstalledEthernetSwitchExtension installedEthernetSwitchExtension = ObjectLocator.GetInstalledEthernetSwitchExtension(base.Server, ExtensionId);
        if (installedEthernetSwitchExtension != null)
        {
            ExtensionName = installedEthernetSwitchExtension.FriendlyName;
        }
        m_SettingDataObject = ConstructSettingData(featureSetting);
        m_FeatureService = featureService;
    }

    private ICimInstance ConstructSettingData(IEthernetFeature featureSetting)
    {
        return featureSetting.Server.GetInstance(featureSetting.ManagementPath);
    }

    internal string GetEmbeddedInstance()
    {
        return base.Server.GetEmbeddedInstance(m_SettingDataObject);
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformOperation(() => m_FeatureService.BeginModifyFeatures(new string[1] { GetEmbeddedInstance() }), m_FeatureService.EndModifyFeatures, DescriptionForPut, this);
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformOperation(() => m_FeatureService.BeginRemoveFeatures(new IEthernetFeature[1] { m_FeatureSetting }), m_FeatureService.EndRemoveFeatures, DescriptionForRemove, this);
        ResetParentFeatureCache();
    }

    protected abstract void ResetParentFeatureCache();

    internal static IEnumerable<TFeature> FilterFeatureSettings<TFeature>(IEnumerable<TFeature> features, Guid[] featureIds, string[] featureNames, VMSwitchExtension[] extensions, VMSystemSwitchExtension[] systemExtensions, string[] extensionNames) where TFeature : VMSwitchExtensionCustomFeature
    {
        if (featureIds != null)
        {
            features = features.Where((TFeature feature) => feature.Id.HasValue && featureIds.Contains(feature.Id.Value));
        }
        else if (featureNames != null)
        {
            WildcardPatternMatcher featureMatcher = new WildcardPatternMatcher(featureNames);
            features = features.Where((TFeature feature) => featureMatcher.MatchesAny(feature.Name));
        }
        if (extensions != null)
        {
            features = features.Where((TFeature feature) => extensions.Any((VMSwitchExtension extension) => string.Equals(extension.Id, feature.ExtensionId, StringComparison.OrdinalIgnoreCase)));
        }
        else if (systemExtensions != null)
        {
            features = features.Where((TFeature feature) => systemExtensions.Any((VMSystemSwitchExtension systemExtension) => string.Equals(systemExtension.Id, feature.ExtensionId, StringComparison.OrdinalIgnoreCase)));
        }
        else if (extensionNames != null)
        {
            WildcardPatternMatcher extensionMatcher = new WildcardPatternMatcher(extensionNames);
            features = features.Where((TFeature status) => extensionMatcher.MatchesAny(status.ExtensionName));
        }
        return features;
    }
}
