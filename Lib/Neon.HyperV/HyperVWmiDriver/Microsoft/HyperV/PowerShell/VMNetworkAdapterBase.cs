using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMNetworkAdapterBase : VMDevice, IAddable, IRemovable
{
    protected IDataUpdater<IEthernetPortSetting> m_PortSetting;

    protected IDataUpdater<IEthernetPortAllocationSettingData> m_ConnectionSetting;

    public abstract string SwitchName { get; }

    public abstract string AdapterId { get; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public string[] StatusDescription
    {
        get
        {
            ILanEndpoint lanEndpoint = GetLanEndpoint();
            if (lanEndpoint == null)
            {
                return null;
            }
            if (IsDownlevelGuestIC(lanEndpoint))
            {
                return new string[1] { VMNetworkAdapterOperationalStatus.Ok.ToString() };
            }
            return lanEndpoint.StatusDescriptions;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public VMNetworkAdapterOperationalStatus[] Status
    {
        get
        {
            ILanEndpoint lanEndpoint = GetLanEndpoint();
            if (lanEndpoint == null)
            {
                return null;
            }
            if (IsDownlevelGuestIC(lanEndpoint))
            {
                return new VMNetworkAdapterOperationalStatus[1] { VMNetworkAdapterOperationalStatus.Ok };
            }
            return lanEndpoint.OperationalStatus.Select((VMLanEndpointOperationalStatus status) => (VMNetworkAdapterOperationalStatus)status).ToArray();
        }
    }

    public virtual bool IsManagementOs => false;

    public virtual bool IsExternalAdapter => false;

    public override string Id
    {
        get
        {
            if (IsExternalAdapter || IsManagementOs)
            {
                return m_ConnectionSetting?.GetData(UpdatePolicy.EnsureUpdated).DeviceId;
            }
            return base.Id;
        }
    }

    public Guid? SwitchId
    {
        get
        {
            VMSwitch connectedSwitch = GetConnectedSwitch();
            return (!(connectedSwitch != null)) ? null : new Guid?(connectedSwitch.Id);
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Usability is more important than the slight gain in efficiency here.")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Usability is more important than the slight gain in efficiency here.")]
    public List<VMNetworkAdapterAclSetting> AclList => (from acl in GetFeatureSettings().OfType<IEthernetSwitchPortAclFeature>()
        select new VMNetworkAdapterAclSetting(acl, this)).ToList();

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Usability is more important than the slight gain in efficiency here.")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Usability is more important than the slight gain in efficiency here.")]
    public List<VMNetworkAdapterExtendedAclSetting> ExtendedAclList => (from eacl in GetFeatureSettings().OfType<IEthernetSwitchPortExtendedAclFeature>()
        select new VMNetworkAdapterExtendedAclSetting(eacl, this)).ToList();

    public VMNetworkAdapterIsolationSetting IsolationSetting
    {
        get
        {
            IEthernetSwitchPortIsolationFeature ethernetSwitchPortIsolationFeature = GetFeatureSettings().OfType<IEthernetSwitchPortIsolationFeature>().SingleOrDefault();
            if (ethernetSwitchPortIsolationFeature != null)
            {
                return new VMNetworkAdapterIsolationSetting(ethernetSwitchPortIsolationFeature, this);
            }
            return VMNetworkAdapterIsolationSetting.CreateTemplateIsolationSetting(this);
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Usability is more important than the slight gain in efficiency here.")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Usability is more important than the slight gain in efficiency here.")]
    public List<VMNetworkAdapterRoutingDomainSetting> RoutingDomainList => (from rds in GetFeatureSettings().OfType<IEthernetSwitchPortRoutingDomainFeature>()
        select new VMNetworkAdapterRoutingDomainSetting(rds, this)).ToList();

    public VMNetworkAdapterVlanSetting VlanSetting
    {
        get
        {
            IEthernetSwitchPortVlanFeature ethernetSwitchPortVlanFeature = GetFeatureSettings().OfType<IEthernetSwitchPortVlanFeature>().SingleOrDefault();
            if (ethernetSwitchPortVlanFeature != null)
            {
                return new VMNetworkAdapterVlanSetting(ethernetSwitchPortVlanFeature, this);
            }
            return VMNetworkAdapterVlanSetting.CreateTemplateVlanSetting(this);
        }
    }

    public VMNetworkAdapterBandwidthSetting BandwidthSetting
    {
        get
        {
            IEthernetSwitchPortBandwidthFeature ethernetSwitchPortBandwidthFeature = GetFeatureSettings().OfType<IEthernetSwitchPortBandwidthFeature>().SingleOrDefault();
            if (ethernetSwitchPortBandwidthFeature != null)
            {
                return new VMNetworkAdapterBandwidthSetting(ethernetSwitchPortBandwidthFeature, this);
            }
            return null;
        }
    }

    internal VMNetworkAdapterSecuritySetting SecuritySetting
    {
        get
        {
            IEthernetSwitchPortSecurityFeature ethernetSwitchPortSecurityFeature = GetFeatureSettings().OfType<IEthernetSwitchPortSecurityFeature>().SingleOrDefault();
            if (ethernetSwitchPortSecurityFeature != null)
            {
                return new VMNetworkAdapterSecuritySetting(ethernetSwitchPortSecurityFeature, this);
            }
            return null;
        }
    }

    internal VMNetworkAdapterOffloadSetting OffloadSetting
    {
        get
        {
            IEthernetSwitchPortOffloadFeature ethernetSwitchPortOffloadFeature = GetFeatureSettings().OfType<IEthernetSwitchPortOffloadFeature>().SingleOrDefault();
            if (ethernetSwitchPortOffloadFeature != null)
            {
                return new VMNetworkAdapterOffloadSetting(ethernetSwitchPortOffloadFeature, this);
            }
            return null;
        }
    }

    internal VMNetworkAdapterRdmaSetting RdmaSetting
    {
        get
        {
            IEthernetSwitchPortRdmaFeature ethernetSwitchPortRdmaFeature = GetFeatureSettings().OfType<IEthernetSwitchPortRdmaFeature>().SingleOrDefault();
            if (ethernetSwitchPortRdmaFeature != null)
            {
                return new VMNetworkAdapterRdmaSetting(ethernetSwitchPortRdmaFeature, this);
            }
            return null;
        }
    }

    internal VMNetworkAdapterTeamMappingSetting TeamMappingSetting
    {
        get
        {
            IEthernetSwitchPortTeamMappingFeature ethernetSwitchPortTeamMappingFeature = GetFeatureSettings().OfType<IEthernetSwitchPortTeamMappingFeature>().SingleOrDefault();
            if (ethernetSwitchPortTeamMappingFeature != null)
            {
                return new VMNetworkAdapterTeamMappingSetting(ethernetSwitchPortTeamMappingFeature, this);
            }
            return null;
        }
    }

    public VMNetworkAdapterIsolationMode CurrentIsolationMode
    {
        get
        {
            VMNetworkAdapterIsolationMode result = VMNetworkAdapterIsolationMode.Vlan;
            if (VirtualSubnetId != 0)
            {
                result = VMNetworkAdapterIsolationMode.NativeVirtualSubnet;
            }
            else
            {
                VMNetworkAdapterIsolationMode isolationMode = IsolationSetting.IsolationMode;
                if (isolationMode != 0)
                {
                    result = isolationMode;
                }
            }
            return result;
        }
    }

    public OnOffState MacAddressSpoofing => SecuritySetting?.MacAddressSpoofing ?? OnOffState.Off;

    public OnOffState DhcpGuard => SecuritySetting?.DhcpGuard ?? OnOffState.Off;

    public OnOffState RouterGuard => SecuritySetting?.RouterGuard ?? OnOffState.Off;

    public VMNetworkAdapterPortMirroringMode PortMirroringMode => SecuritySetting?.PortMirroringMode ?? VMNetworkAdapterPortMirroringMode.None;

    public OnOffState IeeePriorityTag => SecuritySetting?.IeeePriorityTag ?? OnOffState.Off;

    public uint VirtualSubnetId => SecuritySetting?.VirtualSubnetId ?? 0;

    public uint DynamicIPAddressLimit => SecuritySetting?.DynamicIPAddressLimit ?? 0;

    public uint StormLimit => SecuritySetting?.StormLimit ?? 0;

    public OnOffState AllowTeaming => SecuritySetting?.AllowTeaming ?? OnOffState.Off;

    public OnOffState FixSpeed10G => SecuritySetting?.FixSpeed10G ?? OnOffState.Off;

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VMQ", Justification = "This is per spec.")]
    public uint VMQWeight => OffloadSetting?.VMQWeight ?? 0;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Psec", Justification = "IPsec is a standard term.")]
    public long IPsecOffloadMaxSA
    {
        get
        {
            VMNetworkAdapterOffloadSetting offloadSetting = OffloadSetting;
            if (offloadSetting != null)
            {
                return offloadSetting.IPSecOffloadMaxSA;
            }
            return 0L;
        }
    }

    public bool VrssEnabled => GetSwitchPortOffloadStatus()?.VrssEnabled ?? true;

    public bool VrssEnabledRequested => OffloadSetting?.VrssEnabled ?? true;

    public bool VmmqEnabled => GetSwitchPortOffloadStatus()?.VmmqEnabled ?? true;

    public bool VmmqEnabledRequested => OffloadSetting?.VmmqEnabled ?? true;

    public uint VrssMaxQueuePairs => GetSwitchPortOffloadStatus()?.VmmqQueuePairs ?? 0;

    public uint VrssMaxQueuePairsRequested => OffloadSetting?.VrssMaxQueuePairs ?? 0;

    public uint VrssMinQueuePairs => GetSwitchPortOffloadStatus()?.VrssMinQueuePairs ?? 0;

    public uint VrssMinQueuePairsRequested => OffloadSetting?.VrssMinQueuePairs ?? 0;

    public VrssQueueSchedulingModeType VrssQueueSchedulingMode => (VrssQueueSchedulingModeType)(GetSwitchPortOffloadStatus()?.VrssQueueSchedulingMode ?? 2);

    public VrssQueueSchedulingModeType VrssQueueSchedulingModeRequested => OffloadSetting?.VrssQueueSchedulingMode ?? VrssQueueSchedulingModeType.StaticVrss;

    public bool VrssExcludePrimaryProcessor => GetSwitchPortOffloadStatus()?.VrssExcludePrimaryProcessor ?? false;

    public bool VrssExcludePrimaryProcessorRequested => OffloadSetting?.VrssExcludePrimaryProcessor ?? false;

    public bool VrssIndependentHostSpreading => GetSwitchPortOffloadStatus()?.VrssIndependentHostSpreading ?? false;

    public bool VrssIndependentHostSpreadingRequested => OffloadSetting?.VrssIndependentHostSpreading ?? false;

    public VrssVmbusChannelAffinityPolicyType VrssVmbusChannelAffinityPolicy => (VrssVmbusChannelAffinityPolicyType)(GetSwitchPortOffloadStatus()?.VrssVmbusChannelAffinityPolicy ?? 3);

    public VrssVmbusChannelAffinityPolicyType VrssVmbusChannelAffinityPolicyRequested => OffloadSetting?.VrssVmbusChannelAffinityPolicy ?? VrssVmbusChannelAffinityPolicyType.Strong;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vmq", Justification = "This is by spec.")]
    public int VmqUsage => GetSwitchPortOffloadStatus()?.VmqOffloadUsage ?? 0;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Psec", Justification = "IPsec is a standard term.")]
    public uint IPsecOffloadSAUsage => GetSwitchPortOffloadStatus()?.IpsecCurrentOffloadSaCount ?? 0;

    public bool VFDataPathActive => GetSwitchPortOffloadStatus()?.IovOffloadActive ?? false;

    public CimInstance VMQueue
    {
        get
        {
            CimInstance result = null;
            IEthernetSwitchPortOffloadStatus switchPortOffloadStatus = GetSwitchPortOffloadStatus();
            if (switchPortOffloadStatus != null && switchPortOffloadStatus.VmqOffloadUsage > 0)
            {
                VMSwitch connectedSwitch = GetConnectedSwitch();
                if (connectedSwitch != null)
                {
                    string text = connectedSwitch.NetAdapterInterfaceDescriptions?.FirstOrDefault();
                    if (!string.IsNullOrEmpty(text))
                    {
                        ICimInstance vMQueue = NetworkingUtilities.GetVMQueue(base.Server, text, switchPortOffloadStatus.VmqId);
                        if (vMQueue != null)
                        {
                            result = vMQueue.Instance;
                        }
                    }
                }
            }
            return result;
        }
    }

    public uint BandwidthPercentage => GetSwitchPortBandwidthStatus()?.CurrentBandwidthReservationPercentage ?? 0;

    internal VMSwitchBandwidthMode BandwidthReservationMode
    {
        get
        {
            VMSwitch connectedSwitch = GetConnectedSwitch();
            if (connectedSwitch != null)
            {
                return connectedSwitch.BandwidthReservationMode;
            }
            return VMSwitchBandwidthMode.None;
        }
    }

    public bool IsTemplate { get; protected set; }

    internal abstract IEthernetSwitchFeatureService FeatureService { get; }

    internal VMNetworkAdapterBase(IEthernetPortSetting portSetting, IEthernetConnectionAllocationRequest connectionSetting, ComputeResource parentVirtualMachineObject, bool isTemplate)
        : base(portSetting, parentVirtualMachineObject)
    {
        if (isTemplate)
        {
            m_PortSetting = new TemplateObjectDataUpdater<IEthernetPortSetting>(portSetting);
            m_ConnectionSetting = new TemplateObjectDataUpdater<IEthernetConnectionAllocationRequest>(connectionSetting);
        }
        else
        {
            m_PortSetting = InitializePrimaryDataUpdater(portSetting);
            m_ConnectionSetting = new DependentObjectDataUpdater<IEthernetConnectionAllocationRequest>(connectionSetting, delegate
            {
                portSetting.UpdateAssociationCache();
                return portSetting.GetConnectionConfiguration();
            });
        }
        IsTemplate = isTemplate;
    }

    internal VMNetworkAdapterBase(IVirtualEthernetSwitchPortSetting connectionSetting)
        : base(connectionSetting, null)
    {
        m_PortSetting = null;
        m_ConnectionSetting = InitializePrimaryDataUpdater(connectionSetting);
    }

    public override string ToString()
    {
        if (GetParentAs<ComputeResource>() != null)
        {
            return base.ToString();
        }
        return string.Format(CultureInfo.InvariantCulture, "{0}, Name = '{1}'", GetType().Name, Name);
    }

    internal IEnumerable<IEthernetSwitchPortFeature> GetFeatureSettings()
    {
        IEthernetPortAllocationSettingData data = m_ConnectionSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated);
        if (data != null)
        {
            return data.Features;
        }
        return Enumerable.Empty<IEthernetSwitchPortFeature>();
    }

    internal void InvalidateFeatureCache()
    {
        m_ConnectionSetting.GetData(UpdatePolicy.None)?.InvalidateAssociationCache();
    }

    internal IEnumerable<TFeatureSetting> AddFeatureSettings<TFeatureSetting>(IList<TFeatureSetting> features, IOperationWatcher operationWatcher) where TFeatureSetting : VMNetworkAdapterFeatureBase
    {
        IEnumerable<IEthernetSwitchPortFeature> features2 = features.Select((TFeatureSetting feature) => (IEthernetSwitchPortFeature)feature.m_FeatureSetting);
        IEthernetSwitchFeatureService featureService = FeatureService;
        return (from feature in AddFeatureSettingsInternal(operationWatcher, features2, featureService.BeginAddPortFeatures, featureService.EndAddPortFeatures)
            select VMNetworkAdapterFeatureBase.Create(this, feature)).Cast<TFeatureSetting>();
    }

    internal IEnumerable<VMSwitchExtensionPortFeature> AddCustomFeatureSettings(IList<VMSwitchExtensionPortFeature> features, IOperationWatcher operationWatcher)
    {
        IEnumerable<string> features2 = features.Select((VMSwitchExtensionPortFeature feature) => feature.GetEmbeddedInstance());
        IEthernetSwitchFeatureService featureService = FeatureService;
        return from feature in AddFeatureSettingsInternal(operationWatcher, features2, featureService.BeginAddPortFeatures, featureService.EndAddPortFeatures)
            select new VMSwitchExtensionPortFeature(feature, this);
    }

    internal TFeatureSetting AddOrModifyOneFeatureSetting<TFeatureSetting>(TFeatureSetting feature, IOperationWatcher operationWatcher) where TFeatureSetting : VMNetworkAdapterFeatureBase
    {
        if (feature.IsTemplate)
        {
            return AddFeatureSettings(new TFeatureSetting[1] { feature }, operationWatcher).Single();
        }
        ((IUpdatable)feature).Put(operationWatcher);
        return feature;
    }

    private IEnumerable<IEthernetSwitchPortFeature> AddFeatureSettingsInternal<TFeatureRepresentation>(IOperationWatcher operationWatcher, IEnumerable<TFeatureRepresentation> features, Func<IEthernetPortAllocationSettingData, TFeatureRepresentation[], IVMTask> beginAddTaskMethod, Func<IVMTask, IEnumerable<IEthernetSwitchPortFeature>> endAddTaskMethod)
    {
        IEthernetPortAllocationSettingData connection = m_ConnectionSetting.GetData(UpdatePolicy.None);
        if (connection == null)
        {
            throw ExceptionHelper.CreateInvalidStateException(ErrorMessages.VMNetworkAdapter_ConnectionNotFound, null, this);
        }
        IEnumerable<IEthernetSwitchPortFeature> result = operationWatcher.PerformOperationWithReturn(() => beginAddTaskMethod(connection, features.ToArray()), endAddTaskMethod, TaskDescriptions.SetVMNetworkAdapterFeature, this);
        connection.InvalidateAssociationCache();
        return result;
    }

    private static bool IsDownlevelGuestIC(ILanEndpoint lanEndpoint)
    {
        VMLanEndpointOperationalStatus[] operationalStatus = lanEndpoint.OperationalStatus;
        if (operationalStatus.Length >= 2 && operationalStatus[0] == VMLanEndpointOperationalStatus.Degraded)
        {
            return operationalStatus[1] == VMLanEndpointOperationalStatus.ProtocolMismatch;
        }
        return false;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        RemoveSelf(operationWatcher);
    }

    internal virtual void PrepareForModify(IOperationWatcher operationWatcher)
    {
    }

    internal abstract void RemoveSelf(IOperationWatcher operationWatcher);

    internal abstract ILanEndpoint GetLanEndpoint();

    internal abstract VMSwitch GetConnectedSwitch();

    internal abstract IVirtualEthernetSwitchPort GetSwitchPort();

    internal IEthernetSwitchPortOffloadStatus GetSwitchPortOffloadStatus()
    {
        IVirtualEthernetSwitchPort switchPort = GetSwitchPort();
        if (switchPort != null)
        {
            switchPort.UpdateAssociationCache(Constants.UpdateThreshold);
            return switchPort.OffloadStatus;
        }
        return null;
    }

    internal IEthernetSwitchPortBandwidthStatus GetSwitchPortBandwidthStatus()
    {
        IVirtualEthernetSwitchPort switchPort = GetSwitchPort();
        if (switchPort != null)
        {
            switchPort.UpdateAssociationCache(Constants.UpdateThreshold);
            return switchPort.BandwidthStatus;
        }
        return null;
    }

    internal IList<VMSwitchExtensionPortData> GetRuntimeStatuses()
    {
        IVirtualEthernetSwitchPort switchPort = GetSwitchPort();
        if (switchPort != null)
        {
            switchPort.UpdateAssociationCache(Constants.UpdateThreshold);
            return (from status in switchPort.GetRuntimeStatuses()
                select new VMSwitchExtensionPortData(status, this)).ToList();
        }
        return new VMSwitchExtensionPortData[0];
    }

    internal IList<VMSwitchExtensionPortFeature> GetPortFeatures()
    {
        return (from feature in GetFeatureSettings()
            select new VMSwitchExtensionPortFeature(feature, this)).ToList();
    }
}
