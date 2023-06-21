using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitch : VirtualizationObject, IRemovable, IUpdatable, IEquatable<VMSwitch>, IVMNetworkAdapterOwner
{
    private readonly IDataUpdater<IVirtualEthernetSwitchSetting> m_SwitchSetting;

    private readonly IDataUpdater<IVirtualEthernetSwitch> m_VirtualSwitch;

    [VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
    public string Name
    {
        get
        {
            return m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).SwitchFriendlyName;
        }
        internal set
        {
            m_SwitchSetting.GetData(UpdatePolicy.None).SwitchFriendlyName = value;
        }
    }

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
    public Guid Id => m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).Id;

    public string Notes
    {
        get
        {
            return m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).Notes;
        }
        internal set
        {
            m_SwitchSetting.GetData(UpdatePolicy.None).Notes = value;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public VMSwitchExtension[] Extensions => m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).ExtensionList.Select((IEthernetSwitchExtension extension) => new VMSwitchExtension(extension, this)).ToArray();

    public VMSwitchBandwidthMode BandwidthReservationMode => (VMSwitchBandwidthMode)m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).BandwidthReservationMode;

    public bool PacketDirectEnabled => m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).PacketDirectEnabled;

    public bool EmbeddedTeamingEnabled => m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).TeamingEnabled;

    public bool IovEnabled
    {
        get
        {
            return m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated).IOVPreferred;
        }
        internal set
        {
            m_SwitchSetting.GetData(UpdatePolicy.None).IOVPreferred = value;
        }
    }

    public VMSwitchType SwitchType
    {
        get
        {
            m_SwitchSetting.GetData(UpdatePolicy.EnsureUpdated);
            GetAndOrganizeNetworkAdapters(out var externalPort, out var internalPorts);
            if (externalPort != null)
            {
                return VMSwitchType.External;
            }
            if (internalPorts.Any())
            {
                return VMSwitchType.Internal;
            }
            return VMSwitchType.Private;
        }
    }

    public bool AllowManagementOS
    {
        get
        {
            GetAndOrganizeNetworkAdapters(out var _, out var internalPorts);
            return internalPorts.Any();
        }
    }

    public string NetAdapterInterfaceDescription
    {
        get
        {
            if (!EmbeddedTeamingEnabled)
            {
                if (NetAdapterInterfaceDescriptions != null)
                {
                    return NetAdapterInterfaceDescriptions.FirstOrDefault();
                }
                return null;
            }
            return ObjectDescriptions.VMSwitch_SETInterfaceDescription;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string[] NetAdapterInterfaceDescriptions
    {
        get
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var _);
            if (externalPort != null)
            {
                if (!EmbeddedTeamingEnabled)
                {
                    return new string[1] { externalPort.InterfaceDescription };
                }
                return (from extNic in externalPort.GetConnectedExternalAdapters()
                    select extNic.FriendlyName).ToArray();
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public Guid[] NetAdapterInterfaceGuid
    {
        get
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var _);
            return externalPort?.InterfaceIds.ToArray();
        }
    }

    public bool IovSupport
    {
        get
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var _);
            return externalPort?.IovSupport ?? false;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string[] IovSupportReasons
    {
        get
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var _);
            return externalPort?.IovSupportReasons.ToArray();
        }
    }

    public uint AvailableIPSecSA => GetOffloadStatus().IPsecSACapacity;

    public uint NumberIPSecSAAllocated => GetOffloadStatus().IPsecSAUsage;

    public uint AvailableVMQueues => GetOffloadStatus().VmqCapacity;

    public uint NumberVmqAllocated => GetOffloadStatus().VmqUsage;

    public uint IovQueuePairCount => GetOffloadStatus().IovQueuePairCapacity;

    public uint IovQueuePairsInUse => GetOffloadStatus().IovQueuePairUsage;

    public uint IovVirtualFunctionCount => GetOffloadStatus().IovVfCapacity;

    public uint IovVirtualFunctionsInUse => GetOffloadStatus().IovVfUsage;

    public bool PacketDirectInUse => GetOffloadStatus().PacketDirectInUse;

    public bool DefaultQueueVrssEnabledRequested => OffloadSetting?.DefaultQueueVrssEnabled ?? true;

    public bool DefaultQueueVrssEnabled => GetOffloadStatus().DefaultQueueVrssEnabled;

    public bool DefaultQueueVmmqEnabledRequested => OffloadSetting?.DefaultQueueVmmqEnabled ?? true;

    public bool DefaultQueueVmmqEnabled => GetOffloadStatus().DefaultQueueVmmqEnabled;

    public uint DefaultQueueVrssMaxQueuePairsRequested => OffloadSetting?.DefaultQueueVrssMaxQueuePairs ?? 16;

    public uint DefaultQueueVrssMaxQueuePairs => GetOffloadStatus().DefaultQueueVmmqQueuePairs;

    public uint DefaultQueueVrssMinQueuePairsRequested => OffloadSetting?.DefaultQueueVrssMinQueuePairs ?? 1;

    public uint DefaultQueueVrssMinQueuePairs => GetOffloadStatus().DefaultQueueVrssMinQueuePairs;

    public VrssQueueSchedulingModeType DefaultQueueVrssQueueSchedulingModeRequested => OffloadSetting?.DefaultQueueVrssQueueSchedulingMode ?? VrssQueueSchedulingModeType.StaticVrss;

    public VrssQueueSchedulingModeType DefaultQueueVrssQueueSchedulingMode => (VrssQueueSchedulingModeType)GetOffloadStatus().DefaultQueueVrssQueueSchedulingMode;

    public bool DefaultQueueVrssExcludePrimaryProcessorRequested => OffloadSetting?.DefaultQueueVrssExcludePrimaryProcessor ?? false;

    public bool DefaultQueueVrssExcludePrimaryProcessor => GetOffloadStatus().DefaultQueueVrssExcludePrimaryProcessor;

    public bool SoftwareRscEnabled => OffloadSetting?.SoftwareRscEnabled ?? false;

    public uint BandwidthPercentage => GetBandwidthStatus().DefaultFlowReservationPercentage;

    public ulong DefaultFlowMinimumBandwidthAbsolute => GetBandwidthStatus().DefaultFlowReservation;

    public ulong DefaultFlowMinimumBandwidthWeight => GetBandwidthStatus().DefaultFlowWeight;

    internal IVirtualEthernetSwitch VirtualizationManagementSwitch => m_VirtualSwitch.GetData(UpdatePolicy.None);

    internal VMSwitchBandwidthSetting BandwidthSetting
    {
        get
        {
            IEthernetSwitchBandwidthFeature ethernetSwitchBandwidthFeature = GetFeatureSettings().OfType<IEthernetSwitchBandwidthFeature>().SingleOrDefault();
            if (ethernetSwitchBandwidthFeature != null)
            {
                return new VMSwitchBandwidthSetting(ethernetSwitchBandwidthFeature, this);
            }
            return null;
        }
    }

    internal VMSwitchNicTeamingSetting NicTeamingSetting
    {
        get
        {
            IEthernetSwitchNicTeamingFeature ethernetSwitchNicTeamingFeature = GetFeatureSettings().OfType<IEthernetSwitchNicTeamingFeature>().SingleOrDefault();
            if (ethernetSwitchNicTeamingFeature != null)
            {
                return new VMSwitchNicTeamingSetting(ethernetSwitchNicTeamingFeature, this);
            }
            return null;
        }
    }

    internal VMSwitchOffloadSetting OffloadSetting
    {
        get
        {
            IEthernetSwitchOffloadFeature ethernetSwitchOffloadFeature = GetFeatureSettings().OfType<IEthernetSwitchOffloadFeature>().SingleOrDefault();
            if (ethernetSwitchOffloadFeature != null)
            {
                return new VMSwitchOffloadSetting(ethernetSwitchOffloadFeature, this);
            }
            return null;
        }
    }

    internal VMSwitch(IVirtualEthernetSwitchSetting switchSetting)
        : base(switchSetting)
    {
        m_SwitchSetting = InitializePrimaryDataUpdater(switchSetting);
        IVirtualEthernetSwitch @switch = m_SwitchSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Switch;
        m_VirtualSwitch = new DataUpdater<IVirtualEthernetSwitch>(@switch);
    }

    internal IEnumerable<IEthernetSwitchFeature> GetFeatureSettings()
    {
        return m_SwitchSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Features;
    }

    internal void SetExtensionOrder(IEnumerable<VMSwitchExtension> extensions)
    {
        m_SwitchSetting.GetData(UpdatePolicy.None).ExtensionList = extensions.Select((VMSwitchExtension extension) => extension.VirtualizationManagementExtension).ToList();
    }

    internal IList<VMSwitchExtensionSwitchData> GetRuntimeStatuses()
    {
        return (from status in m_VirtualSwitch.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetRuntimeStatuses()
            select new VMSwitchExtensionSwitchData(status, this)).ToList();
    }

    internal IList<VMSwitchExtensionSwitchFeature> GetSwitchFeatures()
    {
        return (from feature in GetFeatureSettings()
            select new VMSwitchExtensionSwitchFeature(feature, this)).ToList();
    }

    internal VMExternalNetworkAdapter GetExternalPort()
    {
        GetAndOrganizeNetworkAdapters(out var externalPort, out var _);
        return externalPort;
    }

    internal IList<VMInternalNetworkAdapter> GetInternalPorts()
    {
        GetAndOrganizeNetworkAdapters(out var _, out var internalPorts);
        return internalPorts;
    }

    internal VMInternalNetworkAdapter AddInternalNetworkAdapter(string name, string macAddress, IOperationWatcher operationWatcher)
    {
        IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(base.Server);
        IEthernetPortAllocationSettingData portToAdd = NetworkingUtilities.CreateTemplateSwitchPortSetting(hostComputerSystem, name, new WmiObjectPath[1] { hostComputerSystem.ManagementPath }, macAddress);
        IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        IVirtualEthernetSwitchPortSetting obj = (IVirtualEthernetSwitchPortSetting)operationWatcher.PerformOperationWithReturn(() => switchService.BeginAddVirtualSwitchPorts(VirtualizationManagementSwitch, new IEthernetPortAllocationSettingData[1] { portToAdd }), switchService.EndAddVirtualSwitchPorts, TaskDescriptions.SetVMSwitch, this).Single();
        obj.UpdateAssociationCache();
        switchService.UpdateInternalEthernetPorts(TimeSpan.Zero);
        IVirtualEthernetSwitchPort virtualSwitchPort = obj.VirtualSwitchPort;
        return new VMInternalNetworkAdapter(obj, virtualSwitchPort, (IInternalEthernetPort)virtualSwitchPort.GetConnectedEthernetPort(TimeSpan.Zero), this);
    }

    internal void ConfigureConnections(VMSwitchType switchType, bool allowManagementOS, string[] externalNicName, string[] externalNicDescription, IOperationWatcher operationWatcher)
    {
        VMSwitchType switchType2 = SwitchType;
        bool allowManagementOS2 = AllowManagementOS;
        if (switchType2 != switchType)
        {
            ChangeSwitchType(switchType, switchType2, allowManagementOS, externalNicName, externalNicDescription, operationWatcher);
        }
        else if (switchType2 == VMSwitchType.External)
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var internalPorts);
            if (externalPort == null)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMSwitch_ExternalPortNotFound, null);
            }
            string macAddress = externalPort.ExternalAdapterMacAddress;
            if (!externalNicName.IsNullOrEmpty() || !externalNicDescription.IsNullOrEmpty())
            {
                IExternalNetworkPort[] array = NetworkingUtilities.FindExternalNetworkPorts(base.Server, externalNicName, externalNicDescription, TimeSpan.Zero);
                externalPort.SetConnectedExternalAdapters(array);
                macAddress = array[0].PermanentAddress;
                ((IUpdatable)externalPort).Put(operationWatcher);
            }
            if (allowManagementOS2 != allowManagementOS)
            {
                if (allowManagementOS)
                {
                    AddInternalNetworkAdapter(Name, macAddress, operationWatcher);
                }
                else
                {
                    DisconnectSwitchPorts(base.Server, null, internalPorts, disconnectInternalOnly: true);
                }
            }
        }
        VirtualizationManagementSwitch.InvalidateAssociationCache();
    }

    internal IEnumerable<TFeatureSetting> AddFeatureSettings<TFeatureSetting>(IList<TFeatureSetting> features, IOperationWatcher operationWatcher) where TFeatureSetting : VMSwitchFeatureBase
    {
        IEnumerable<IEthernetSwitchFeature> features2 = features.Select((TFeatureSetting feature) => (IEthernetSwitchFeature)feature.m_FeatureSetting);
        IVirtualSwitchManagementService virtualSwitchManagementService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        return (from feature in AddFeatureSettingsInternal(operationWatcher, features2, virtualSwitchManagementService.BeginAddSwitchFeatures, virtualSwitchManagementService.EndAddSwitchFeatures)
            select VMSwitchFeatureBase.Create(this, feature)).Cast<TFeatureSetting>();
    }

    internal IEnumerable<VMSwitchExtensionSwitchFeature> AddCustomFeatureSettings(IList<VMSwitchExtensionSwitchFeature> features, IOperationWatcher operationWatcher)
    {
        IEnumerable<string> features2 = features.Select((VMSwitchExtensionSwitchFeature feature) => feature.GetEmbeddedInstance());
        IVirtualSwitchManagementService virtualSwitchManagementService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        return from feature in AddFeatureSettingsInternal(operationWatcher, features2, virtualSwitchManagementService.BeginAddSwitchFeatures, virtualSwitchManagementService.EndAddSwitchFeatures)
            select new VMSwitchExtensionSwitchFeature(feature, this);
    }

    internal TFeatureSetting AddOrModifyOneFeatureSetting<TFeatureSetting>(TFeatureSetting feature, IOperationWatcher operationWatcher) where TFeatureSetting : VMSwitchFeatureBase
    {
        if (feature.IsTemplate)
        {
            return AddFeatureSettings(new TFeatureSetting[1] { feature }, operationWatcher).Single();
        }
        ((IUpdatable)feature).Put(operationWatcher);
        return feature;
    }

    internal void InvalidateFeatureCache()
    {
        m_SwitchSetting.GetData(UpdatePolicy.None).InvalidateAssociationCache();
    }

    private void ChangeSwitchType(VMSwitchType newType, VMSwitchType currentType, bool allowManagementOS, string[] externalNicName, string[] externalNicDescription, IOperationWatcher operationWatcher)
    {
        if (currentType == VMSwitchType.External && IovEnabled)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMSwitch_IovSwitchCannotChangeSwitchType);
        }
        if (newType == VMSwitchType.Private)
        {
            GetAndOrganizeNetworkAdapters(out var externalPort, out var internalPorts);
            DisconnectSwitchPorts(base.Server, externalPort, internalPorts, disconnectInternalOnly: false);
            return;
        }
        if (currentType == VMSwitchType.Private || currentType == VMSwitchType.Internal)
        {
            bool addExternalPort = newType == VMSwitchType.External;
            bool addInternalPort = newType == VMSwitchType.Internal || allowManagementOS;
            AddConnections(operationWatcher, addExternalPort, addInternalPort, externalNicName, externalNicDescription);
            return;
        }
        GetAndOrganizeNetworkAdapters(out var externalPort2, out var internalPorts2);
        string macAddress = externalPort2.ExternalAdapterMacAddress;
        List<VMInternalNetworkAdapter> list = internalPorts2.Where((VMInternalNetworkAdapter port) => string.Equals(macAddress, port.MacAddress, StringComparison.OrdinalIgnoreCase)).ToList();
        int count = list.Count;
        DisconnectSwitchPorts(base.Server, externalPort2, list, disconnectInternalOnly: false);
        if (internalPorts2.Count <= count)
        {
            AddInternalNetworkAdapter(Name, null, operationWatcher);
        }
    }

    internal void AddConnections(IOperationWatcher operationWatcher, bool addExternalPort, bool addInternalPort, string[] externalNicName, string[] externalNicDescription)
    {
        IExternalNetworkPort[] externalNetworkPorts = null;
        if (addExternalPort)
        {
            externalNetworkPorts = NetworkingUtilities.FindExternalNetworkPorts(base.Server, externalNicName, externalNicDescription, Constants.UpdateThreshold);
        }
        bool copyMacAddressToInternalPort = addExternalPort && addInternalPort;
        IEthernetPortAllocationSettingData[] portsToAdd = NetworkingUtilities.CreateTemplateSwitchConnections(base.Server, VirtualizationManagementSwitch, externalNetworkPorts, addExternalPort, addInternalPort, copyMacAddressToInternalPort);
        IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        operationWatcher.PerformOperationWithReturn(() => switchService.BeginAddVirtualSwitchPorts(VirtualizationManagementSwitch, portsToAdd), switchService.EndAddVirtualSwitchPorts, TaskDescriptions.SetVMSwitch, this);
    }

    private IEnumerable<IEthernetSwitchFeature> AddFeatureSettingsInternal<TFeatureRepresentation>(IOperationWatcher operationWatcher, IEnumerable<TFeatureRepresentation> features, Func<IVirtualEthernetSwitchSetting, TFeatureRepresentation[], IVMTask> beginAddTaskMethod, Func<IVMTask, IEnumerable<IEthernetSwitchFeature>> endAddTaskMethod)
    {
        IVirtualEthernetSwitchSetting switchSetting = m_SwitchSetting.GetData(UpdatePolicy.None);
        IEnumerable<IEthernetSwitchFeature> result = operationWatcher.PerformOperationWithReturn(() => beginAddTaskMethod(switchSetting, features.ToArray()), endAddTaskMethod, TaskDescriptions.AddVMNetworkAdapterFeature, this);
        switchSetting.InvalidateAssociationCache();
        return result;
    }

    private void GetAndOrganizeNetworkAdapters(out VMExternalNetworkAdapter externalPort, out IList<VMInternalNetworkAdapter> internalPorts)
    {
        externalPort = null;
        internalPorts = new List<VMInternalNetworkAdapter>();
        foreach (VMInternalOrExternalNetworkAdapter item2 in from port in m_VirtualSwitch.GetData(UpdatePolicy.EnsureAssociatorsUpdated).SwitchPorts
            select VMInternalOrExternalNetworkAdapter.Create(port, this) into networkAdapter
            where networkAdapter != null
            select networkAdapter)
        {
            if (item2 is VMInternalNetworkAdapter item)
            {
                internalPorts.Add(item);
            }
            else
            {
                externalPort = (VMExternalNetworkAdapter)item2;
            }
        }
    }

    private IEthernetSwitchOffloadStatus GetOffloadStatus()
    {
        return m_VirtualSwitch.GetData(UpdatePolicy.EnsureAssociatorsUpdated).OffloadStatus;
    }

    private IEthernetSwitchBandwidthStatus GetBandwidthStatus()
    {
        return m_VirtualSwitch.GetData(UpdatePolicy.EnsureAssociatorsUpdated).BandwidthStatus;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        IVirtualEthernetSwitch ethernetSwitch = m_VirtualSwitch.GetData(UpdatePolicy.EnsureUpdated);
        IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        operationWatcher.PerformOperation(() => switchService.BeginDeleteVirtualSwitch(ethernetSwitch), switchService.EndDeleteVirtualSwitch, TaskDescriptions.RemoveVMSwitch, this);
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(m_SwitchSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMSwitch, this);
    }

    public bool Equals(VMSwitch other)
    {
        if (other == null)
        {
            return false;
        }
        return VirtualizationManagementSwitch.ManagementPath.Equals(other.VirtualizationManagementSwitch.ManagementPath);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (this == obj)
        {
            return true;
        }
        if (obj is VMSwitch)
        {
            return Equals((VMSwitch)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return VirtualizationManagementSwitch.GetHashCode();
    }

    public static bool operator ==(VMSwitch left, VMSwitch right)
    {
        return object.Equals(left, right);
    }

    public static bool operator !=(VMSwitch left, VMSwitch right)
    {
        return !object.Equals(left, right);
    }

    internal static IList<VMSwitch> GetSwitchesByNamesAndServers(IEnumerable<Server> servers, string[] requestedSwitchNames, bool allowWildcards, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
    {
        _ = requestedSwitchNames;
        IList<VMSwitch> list = (from virtualSwitch in servers.SelectMany((Server server) => ObjectLocator.QueryObjectsByNames<IVirtualEthernetSwitch>(server, requestedSwitchNames, allowWildcards))
            select new VMSwitch(virtualSwitch.Setting)).ToList();
        if (requestedSwitchNames != null && errorDisplayMode != 0)
        {
            VirtualizationObjectLocator.WriteNonMatchingNameErrors(requestedSwitchNames, list.Select((VMSwitch virtualSwitch) => virtualSwitch.Name), allowWildcards, ErrorMessages.VMSwitch_NotFoundByName, errorDisplayMode, operationWatcher);
        }
        return list;
    }

    internal static VMSwitch Create(Server server, string name, string instanceId, string notes, bool iovEnabled, VMSwitchBandwidthMode bandwidthReservationMode, bool packetDirectEnabled, bool embeddedTeamingEnabled, IOperationWatcher operationWatcher)
    {
        IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(server);
        return new VMSwitch(operationWatcher.PerformOperationWithReturn(() => switchService.BeginCreateVirtualSwitch(name, instanceId, notes, iovEnabled, (BandwidthReservationMode)bandwidthReservationMode, packetDirectEnabled, embeddedTeamingEnabled), switchService.EndCreateVirtualSwitch, TaskDescriptions.NewVMSwitch, null).Setting);
    }

    private static void DisconnectSwitchPorts(Server server, VMExternalNetworkAdapter externalPort, IList<VMInternalNetworkAdapter> internalPorts, bool disconnectInternalOnly)
    {
        IVirtualEthernetSwitchPort externalPort2 = externalPort?.VirtualizationManagementPort;
        List<IVirtualEthernetSwitchPort> internalPorts2 = internalPorts.Select((VMInternalNetworkAdapter port) => port.VirtualizationManagementPort).ToList();
        NetworkingUtilities.DisconnectSwitchInternal(server, externalPort2, internalPorts2, disconnectInternalOnly);
    }
}
