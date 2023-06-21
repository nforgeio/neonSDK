using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Associations
{
    internal readonly WmiAssociation ElementSettingData = new WmiAssociation("Msvm_ElementSettingData");

    internal readonly WmiAssociation LogicalDeviceToSetting = new WmiAssociation("Msvm_SettingsDefineState");

    internal readonly WmiAssociation FrServiceToSetting = new WmiAssociation("Msvm_ElementSettingData", "Msvm_ReplicationServiceSettingData");

    internal readonly WmiAssociation FrServiceToAuthSetting = new WmiAssociation("Msvm_ElementSettingData", "Msvm_ReplicationAuthorizationSettingData");

    internal readonly WmiAssociation SystemToSummary = new WmiAssociation("Msvm_ComputerSystemSummaryInformation", "Msvm_SummaryInformation")
    {
        DoNotUpdate = true
    };

    internal readonly WmiAssociation SummaryToSystem = new WmiAssociation("Msvm_ComputerSystemSummaryInformation", "Msvm_ComputerSystem")
    {
        DoNotUpdate = true
    };

    internal readonly WmiAssociation SystemToSystemSetting = new WmiAssociation("Msvm_SettingsDefineState", "Msvm_VirtualSystemSettingData");

    internal readonly WmiAssociation ResourcePoolToAllocationSetting = new WmiAssociation("Msvm_SettingsDefineState", "CIM_ResourceAllocationSettingData", "SettingData");

    internal readonly WmiAssociation PoolSettingToResourcePool = new WmiAssociation("Msvm_SettingsDefineState", "CIM_ResourcePool", "ManagedElement");

    internal readonly WmiAssociation ResourcePoolToPoolSetting = new WmiAssociation("Msvm_SettingsDefineState", "Msvm_ResourcePoolSettingData", "SettingData");

    internal readonly WmiAssociation VirtualDiskAllocatedFromStoragePool = new WmiAssociation("Msvm_ElementAllocatedFromPool", "Msvm_LogicalDisk", "Dependent");

    internal readonly WmiAssociation SystemSettingToSystem = new WmiAssociation("Msvm_SettingsDefineState", "CIM_ComputerSystem");

    internal readonly WmiAssociation SystemToSnapshotSetting = new WmiAssociation("Msvm_SnapshotOfVirtualSystem", "Msvm_VirtualSystemSettingData");

    internal readonly WmiAssociation SnapshotSettingToSystem = new WmiAssociation("Msvm_SnapshotOfVirtualSystem", "CIM_ComputerSystem");

    internal readonly WmiAssociation SnapshotSettingToChildSetting = new WmiAssociation("Msvm_ParentChildSettingData", "Msvm_VirtualSystemSettingData", "Dependent");

    internal readonly WmiAssociation SystemToReplicationSetting = new WmiAssociation("Msvm_ElementSettingData", "Msvm_ReplicationSettingData");

    internal readonly WmiAssociation SystemToReplicationRelationship = new WmiAssociation("Msvm_SystemReplicationRelationship", "Msvm_ReplicationRelationship");

    internal readonly WmiAssociation SystemToMostRecentlyAppliedSnapshot = new WmiAssociation("Msvm_LastAppliedSnapshot", "Msvm_VirtualSystemSettingData");

    internal readonly WmiAssociation SystemToPreviousSnapshot = new WmiAssociation("Msvm_MostCurrentSnapshotInBranch");

    internal readonly WmiAssociation VirtualMachineProcessor = new WmiAssociation("Msvm_SystemDevice", "Msvm_Processor");

    internal readonly WmiAssociation VirtualMachineMemory = new WmiAssociation("Msvm_SystemDevice", "Msvm_Memory");

    internal readonly WmiAssociation GuestServiceInterfaceComponentToGuestFileService = new WmiAssociation("Msvm_RegisteredGuestService", "Msvm_GuestFileService");

    internal readonly WmiAssociation VirtualMachineHeartbeatComponent = new WmiAssociation("Msvm_SystemDevice", "Msvm_HeartbeatComponent");

    internal readonly WmiAssociation VirtualMachineShutDownComponent = new WmiAssociation("Msvm_SystemDevice", "Msvm_ShutdownComponent");

    internal readonly WmiAssociation VirtualMachineVssComponent = new WmiAssociation("Msvm_SystemDevice", "Msvm_VssComponent");

    internal readonly WmiAssociation SecurityInformationToSecuritySetting = new WmiAssociation("Msvm_SettingsDefineState", "Msvm_SecuritySettingData");

    internal readonly WmiAssociation VirtualMachineSecuritySettingData = new WmiAssociation("Msvm_ConcreteComponent", "Msvm_SecuritySettingData");

    internal readonly WmiAssociation VirtualMachineSecurityInformation = new WmiAssociation("Msvm_SystemComponent", "Msvm_SecurityElement");

    internal readonly WmiAssociation VirtualMachineKeyboard = new WmiAssociation("Msvm_SystemDevice", "Msvm_Keyboard");

    internal readonly WmiAssociation AggregateMemoryToVirtualNumaNode = new WmiAssociation("Msvm_ConcreteComponent", "Msvm_Memory", "PartComponent");

    internal readonly WmiAssociation VirtualMachineMemoryToHostMemory = new WmiAssociation("Msvm_BasedOn", "Msvm_Memory", "Antecedent");

    internal readonly WmiAssociation FcSwitchToSwitchPort = new WmiAssociation("Msvm_SystemDevice", "Msvm_FcSwitchPort");

    internal readonly WmiAssociation FcSwitchPortToSwitch = new WmiAssociation("Msvm_SystemDevice", "Msvm_VirtualFcSwitch");

    internal readonly WmiAssociation EthernetSwitchToSwitchPort = new WmiAssociation("Msvm_SystemDevice", "Msvm_EthernetSwitchPort");

    internal readonly WmiAssociation EthernetSwitchToSwitchRuntimeStatus = new WmiAssociation("Msvm_EthernetSwitchInfo");

    internal readonly WmiAssociation EthernetSwitchPortToSwitch = new WmiAssociation("Msvm_SystemDevice", "Msvm_VirtualEthernetSwitch");

    internal readonly WmiAssociation EthernetSwitchSettingToFsd = new WmiAssociation("Msvm_VirtualEthernetSwitchSettingDataComponent");

    internal readonly WmiAssociation EthernetSwitchPortToPortRuntimeStatus = new WmiAssociation("Msvm_EthernetPortInfo");

    internal readonly WmiAssociation EthernetPortToFsd = new WmiAssociation("Msvm_EthernetPortSettingDataComponent");

    internal readonly WmiAssociation EthernetPortToFailoverNetwork = new WmiAssociation("Msvm_EthernetPortFailoverSettingDataComponent");

    internal readonly WmiAssociation EthernetPortSettingToGuestNetworkAdapterConfiguration = new WmiAssociation("Msvm_SettingDataComponent", "Msvm_GuestNetworkAdapterConfiguration");

    internal readonly WmiAssociation ParentToChildSwitchExtension = new WmiAssociation("Msvm_ParentEthernetSwitchExtension");

    internal readonly WmiAssociation EthernetSwitchExtensionToSwitch = new WmiAssociation("Msvm_HostedEthernetSwitchExtension", "Msvm_VirtualEthernetSwitch");

    internal readonly WmiAssociation FeatureSettingsDefineCapabilities = new WmiAssociation("Msvm_FeatureSettingsDefineCapabilities");

    internal readonly WmiAssociation DevicesSystem = new WmiAssociation("Msvm_SystemDevice", "CIM_ComputerSystem");

    internal readonly WmiAssociation HostedDependency = new WmiAssociation("Msvm_HostedDependency");

    internal readonly WmiAssociation SystemSettingSettingData = new WmiAssociation("Msvm_VirtualSystemSettingDataComponent");

    internal readonly WmiAssociation SystemToExportSettingData = new WmiAssociation("Msvm_SystemExportSettingData", "Msvm_VirtualSystemExportSettingData");

    internal readonly WmiAssociation SystemSettingSettingDataLimited = new WmiAssociation("Msvm_VirtualSystemSettingDataComponent", "Msvm_ResourceAllocationSettingData");

    internal readonly WmiAssociation SystemSettingMemorySettingData = new WmiAssociation("Msvm_VirtualSystemSettingDataComponent", "Msvm_MemorySettingData");

    internal readonly WmiAssociation SystemSettingProcessorSettingData = new WmiAssociation("Msvm_VirtualSystemSettingDataComponent", "Msvm_ProcessorSettingData");

    internal readonly WmiAssociation SystemSettingSyntheticDisplayControllerSettingData = new WmiAssociation("Msvm_VirtualSystemSettingDataComponent", "Msvm_SyntheticDisplayControllerSettingData");

    internal readonly WmiAssociation ActiveConnection = new WmiAssociation("Msvm_ActiveConnection");

    internal readonly WmiAssociation FcEndpointToEndpointAssociation = new WmiAssociation("Msvm_FcActiveConnection");

    internal readonly WmiAssociation FcSwitchPortToEndpointAssociation = new WmiAssociation("Msvm_FcDeviceSAPImplementation", "Msvm_FcEndpoint");

    internal readonly WmiAssociation FcEndpointToSwitchPortAssociation = new WmiAssociation("Msvm_FcDeviceSAPImplementation", "Msvm_FcSwitchPort");

    internal readonly WmiAssociation FcEndpointToExternalFcPortAssociation = new WmiAssociation("Msvm_FcDeviceSAPImplementation", "Msvm_ExternalFcPort");

    internal readonly WmiAssociation ExternalFcPortToFcEndpointAssociation = new WmiAssociation("Msvm_FcDeviceSAPImplementation", "Msvm_FcEndpoint");

    internal readonly WmiAssociation BindsToLanEndpoint = new WmiAssociation("Msvm_BindsToLanEndpoint");

    internal readonly WmiAssociation DeviceSAPImplementation = new WmiAssociation("CIM_DeviceSAPImplementation");

    internal readonly WmiAssociation SettingsDefineState = new WmiAssociation("Msvm_SettingsDefineState");

    internal readonly WmiAssociation ResourcePoolPysicalDevices = new WmiAssociation("Msvm_ConcreteComponent");

    internal readonly WmiAssociation ResourcePoolSystemComponents = new WmiAssociation("Msvm_SystemComponent", "CIM_System", "GroupComponent");

    internal readonly WmiAssociation ResourceAllocationFromPool = new WmiAssociation("Msvm_ResourceAllocationFromPool");

    internal readonly WmiAssociation ParentPools = new WmiAssociation("Msvm_ElementAllocatedFromPool", "CIM_ResourcePool", "Antecedent");

    internal readonly WmiAssociation ChildPools = new WmiAssociation("Msvm_ElementAllocatedFromPool", "CIM_ResourcePool", "Dependent");

    internal readonly WmiAssociation VMMemoryToNumaNode = new WmiAssociation("Msvm_ElementAllocatedFromNumaNode", "Msvm_NumaNode");

    internal readonly WmiAssociation NumaNodeToHostMemory = new WmiAssociation("Msvm_HostedDependency", "Msvm_Memory");

    internal readonly WmiAssociation NumaNodeToHostProcessor = new WmiAssociation("Msvm_HostedDependency", "Msvm_Processor");

    internal readonly WmiAssociation AffectedJobElement = new WmiAssociation("Msvm_AffectedJobElement");

    internal readonly WmiAssociation ElementCapabilities = new WmiAssociation("Msvm_ElementCapabilities");

    internal readonly WmiAssociation SettingsDefineCapabilities = new WmiAssociation("Msvm_SettingsDefineCapabilities")
    {
        DoNotUpdate = true
    };

    internal readonly WmiRelationship SettingsDefineCapabilitiesRelationship = new WmiRelationship("Msvm_SettingsDefineCapabilities")
    {
        DoNotUpdate = true
    };

    internal readonly WmiAssociation ServiceAffectsElement = new WmiAssociation("Msvm_ServiceAffectsElement");

    internal readonly WmiAssociation SerialPortOnSerialController = new WmiAssociation("Msvm_SerialPortOnSerialController");

    internal readonly WmiAssociation ProtocolControllerForUnit = new WmiAssociation("Msvm_ProtocolControllerForUnit");

    internal readonly WmiAssociation TerminalConnections = new WmiAssociation("Msvm_SystemTerminalConnection");

    internal readonly WmiAssociation ReplicaSystemDependency = new WmiAssociation("Msvm_ReplicaSystemDependency");

    internal readonly WmiAssociation MigrationServiceSettingComponent = new WmiAssociation("Msvm_VirtualSystemMigrationServiceSettingDataComponent");

    internal readonly WmiAssociation LogicalIdentity = new WmiAssociation("Msvm_LogicalIdentity");

    internal readonly WmiAssociation ClusterToNode = new WmiAssociation("MSCluster_ClusterToNode");

    internal readonly WmiAssociation ClusterGroupToResource = new WmiAssociation("MSCluster_ResourceGroupToResource");

    internal readonly WmiAssociation SerialControllerSettingToSerialPortSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_SerialPortSettingData");

    internal readonly WmiAssociation EthernetPortSettingToConnectionSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_EthernetPortAllocationSettingData");

    internal readonly WmiAssociation FibreChannelAdapterSettingToConnectionSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_FcPortAllocationSettingData");

    internal readonly WmiAssociation DriveControllerSettingToDriveSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_ResourceAllocationSettingData");

    internal readonly WmiAssociation DriveControllerSettingToKeyStorageDriveSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_VirtualLogicalUnitSettingData");

    internal readonly WmiAssociation DriveSettingToDiskSetting = new WmiAssociation("Msvm_ResourceDependentOnResource", "Msvm_StorageAllocationSettingData");

    internal readonly WmiAssociation VirtualMachineToMigrationJob = new WmiAssociation("Msvm_AffectedJobElement", "Msvm_MigrationJob", "AffectingElement");

    internal readonly WmiAssociation MeasuredElementToMetricValue = new WmiAssociation("Msvm_MetricForME", "CIM_BaseMetricValue", "Dependent");

    internal readonly WmiRelationship MeasuredElementToMetricDefRelationship = new WmiRelationship("Msvm_MetricDefForME", "Antecedent");

    internal readonly WmiAssociation MetricValueToMetricDefinition = new WmiAssociation("Msvm_MetricInstance");

    internal readonly QueryAssociation QueryVirtualSwitch;

    internal readonly QueryAssociation QueryExternalEthernetPorts;

    internal readonly QueryAssociation QueryWiFiPorts;

    internal readonly QueryAssociation QueryInternalEthernetPorts;

    internal readonly QueryAssociation QueryExternalFcPorts;

    internal readonly QueryAssociation QueryResourcePools;

    internal readonly QueryAssociation QueryTasks;

    internal readonly QueryAssociation QueryEthernetSwitchFeatureCapabilities;

    internal readonly QueryAssociation QueryPhysicalProcessors = QueryAssociation.CreateFromClassName(Server.CimV2Namespace, "Win32_Processor");

    internal readonly QueryAssociation QueryWin32CDRomDrives = QueryAssociation.CreateFromQuery(Server.CimV2Namespace, string.Format(CultureInfo.InvariantCulture, "select {0}, {1}, {2} from {3}", "DeviceID", "Drive", "PNPDeviceID", "Win32_CDROMDrive"));

    internal readonly WmiAssociation Win32DirectoryContainsFile = new WmiAssociation("CIM_DirectoryContainsFile");

    internal readonly WmiAssociation Win32SubDirectories = new WmiAssociation("Win32_SubDirectory");

    internal readonly WmiAssociation DependentServices = new WmiAssociation("Win32_DependentService", "Win32_Service", "Dependent");

    internal readonly WmiAssociation ComputerSystemToPartitions = new WmiAssociation("Win32_SystemPartitions", "Win32_DiskPartition", "PartComponent");

    internal readonly WmiAssociation PartitionToLogicalDisk = new WmiAssociation("Win32_LogicalDiskToPartition", "Win32_LogicalDisk", "Dependent");

    internal readonly WmiAssociation LogicalDiskToRootDirectory = new WmiAssociation("Win32_LogicalDiskRootDirectory", "Win32_Directory", "PartComponent");

    internal readonly WmiAssociation Synth3dDisplayControllerToPhysical3dGraphicsProcessor = new WmiAssociation("CIM_HostedDependency", "Msvm_Physical3dGraphicsProcessor");

    internal readonly WmiAssociation CollectingCollections = new WmiAssociation("Msvm_CollectedCollections", "Msvm_ManagementCollection", "Collection");

    internal readonly WmiAssociation CollectedCollections = new WmiAssociation("Msvm_CollectedCollections", "CIM_CollectionOfMSEs", "Member");

    internal readonly WmiAssociation VirtualMachineCollections = new WmiAssociation("Msvm_CollectedVirtualSystems", "Msvm_VirtualSystemCollection", "Collection");

    internal readonly WmiAssociation CollectedVirtualMachines = new WmiAssociation("Msvm_CollectedVirtualSystems", "Msvm_ComputerSystem", "Member");

    internal readonly WmiAssociation VirtualMachineStorageSettingData = new WmiAssociation("Msvm_ConcreteComponent", "Msvm_StorageSettingData");

    private static readonly Associations gm_AssociationsV2 = new Associations("root\\virtualization\\v2");

    private Associations(string namespacePath)
    {
        QueryVirtualSwitch = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_VirtualEthernetSwitch");
        QueryExternalEthernetPorts = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_ExternalEthernetPort");
        QueryWiFiPorts = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_WiFiPort");
        QueryInternalEthernetPorts = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_InternalEthernetPort");
        QueryResourcePools = QueryAssociation.CreateFromClassName(namespacePath, "CIM_ResourcePool");
        QueryTasks = QueryAssociation.CreateFromClassName(namespacePath, "CIM_ConcreteJob");
        QueryExternalFcPorts = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_ExternalFcPort");
        QueryEthernetSwitchFeatureCapabilities = QueryAssociation.CreateFromClassName(namespacePath, "Msvm_EthernetSwitchFeatureCapabilities");
        QueryPhysicalProcessors.DoNotUpdate = true;
    }

    internal static Associations GetAssociations(Server server)
    {
        if (server.VirtualizationNamespaceVersion == WmiNamespaceVersion.V2)
        {
            return gm_AssociationsV2;
        }
        throw new ArgumentOutOfRangeException("server", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_NoSupportedAssociations, server.VirtualizationNamespace));
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
