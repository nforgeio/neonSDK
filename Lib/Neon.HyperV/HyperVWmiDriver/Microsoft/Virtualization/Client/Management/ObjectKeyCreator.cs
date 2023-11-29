using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal static class ObjectKeyCreator
{
    public const string gm_VirtualizationServiceName = "vmms";

    public const string gm_FailoverReplicationServiceName = "replicasvc";

    public const string gm_SnapshotServiceName = "vssnapsvc";

    public const string gm_ImageServiceName = "vhdsvc";

    public const string gm_NetworkingServiceName = "nvspwmi";

    public const string gm_MigrationServiceName = "migrationwmi";

    public const string gm_SecurityServiceName = "vmsecuritywmi";

    public const string gm_ResourcePoolConfigServiceName = "poolcfgsvc";

    public const string gm_Synth3DServiceName = "synth3d";

    public const string gm_TerminalServiceName = "termsvc";

    public const string gm_RDVirtualizationHostName = "VmHostAgent";

    public const string gm_CollectionManagementServiceName = "collectionsvc";

    public const string gm_MetricServiceName = "metricsvc";

    public const string gm_AssignableDeviceServiceName = "vpciehlpsvc";

    public static ObjectKey CreateVMServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_VirtualSystemManagementService", "vmms", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateFailoverReplicationServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_ReplicationService", "replicasvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateSnapshotServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_VirtualSystemSnapshotService", "vssnapsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateSwitchManagementServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_VirtualEthernetSwitchManagementService", "nvspwmi", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateAssignableDeviceService(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_AssignableDeviceService", "vpciehlpsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateResourcePoolConfigServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_ResourcePoolConfigurationService", "poolcfgsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateImageManagementServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_ImageManagementService", "vhdsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateMigrationServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_VirtualSystemMigrationService", "migrationwmi", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateSecurityServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_SecurityService", "vmsecuritywmi", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateMetricServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_MetricService", "metricsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateCollectionManagementServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_CollectionManagementService", "collectionsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateWin32VirtualizationServiceObjectKey(Server server)
    {
        return CreateObjectKey(server, Server.CimV2Namespace, "Win32_Service", "Name", "vmms");
    }

    public static ObjectKey CreateWin32RDVirtualizationHostObjectKey(Server server)
    {
        return CreateObjectKey(server, Server.CimV2Namespace, "Win32_Service", "Name", "VmHostAgent");
    }

    public static ObjectKey CreateCimDataFileObjectKey(Server server, string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return CreateObjectKey(server, Server.CimV2Namespace, "CIM_DataFile", "Name", path);
    }

    public static ObjectKey CreateWin32DirectoryObjectKey(Server server, string directory)
    {
        if (directory == null)
        {
            throw new ArgumentNullException("directory");
        }
        return CreateObjectKey(server, Server.CimV2Namespace, "Win32_Directory", "Name", directory);
    }

    public static ObjectKey CreateTerminalServiceObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return CreateServiceObjectKey(server, "Msvm_TerminalService", "termsvc", "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateTerminalServiceSettingObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        string keyValue = string.Format(CultureInfo.InvariantCulture, "Microsoft:{0}", server.Name);
        return CreateObjectKey(server, server.VirtualizationNamespace, "Msvm_TerminalServiceSettingData", "InstanceID", keyValue);
    }

    public static ObjectKey CreateHostComputerSystemObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return CreateComputerSystemObjectKey(server, server.StandaloneHostVirtualizationNamespace, server.Name, "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateHostClusterComputerSystemObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return CreateComputerSystemObjectKey(server, server.ClusterHostVirtualizationNamespace, server.Name, "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateWin32ComputerSystemObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Name", server.Name } };
        WmiObjectPath path = new WmiObjectPath(server, Server.CimV2Namespace, "Win32_ComputerSystem", keyValues);
        return new ObjectKey(server, path);
    }

    public static ObjectKey CreateVMComputerSystemObjectKey(Server server, string instanceId)
    {
        return CreateComputerSystemObjectKey(server, server.VirtualizationNamespace, instanceId, "Msvm_ComputerSystem");
    }

    public static ObjectKey CreateVMSummaryInformationObjectKey(Server server, string instanceId)
    {
        if (instanceId == null)
        {
            throw new ArgumentNullException("instanceId");
        }
        instanceId = string.Format(CultureInfo.InvariantCulture, "Microsoft:{0}", instanceId);
        return CreateObjectKey(server, server.VirtualizationNamespace, "Msvm_SummaryInformation", "InstanceID", instanceId);
    }

    public static ObjectKey CreateVMPlannedComputerSystemObjectKey(Server server, string instanceId)
    {
        return CreateComputerSystemObjectKey(server, server.VirtualizationNamespace, instanceId, "Msvm_PlannedComputerSystem");
    }

    public static EventObjectKey CreateVMCreationEventObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        WmiObjectPath path = new WmiObjectPath(server, server.VirtualizationNamespace, "Msvm_ComputerSystem");
        return new EventObjectKey(server, path, InstanceEventType.InstanceCreationEvent, null);
    }

    public static EventObjectKey CreateSnapshotCreationEventObjectKey(Server server, string instanceId)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (instanceId == null)
        {
            throw new ArgumentNullException("instanceId");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "VirtualSystemIdentifier", instanceId } };
        WmiObjectPath path = new WmiObjectPath(server, server.VirtualizationNamespace, "Msvm_VirtualSystemSettingData", keyValues);
        return new EventObjectKey(server, path, InstanceEventType.InstanceCreationEvent, new string[1] { string.Format(CultureInfo.InvariantCulture, "{0} LIKE \"{1}%\"", "VirtualSystemType", "Microsoft:Hyper-V:Snapshot:") });
    }

    public static EventObjectKey CreateVMTaskCreationEventObjectKey(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        WmiObjectPath path = new WmiObjectPath(server, server.VirtualizationNamespace, "CIM_ConcreteJob");
        return new EventObjectKey(server, path, InstanceEventType.InstanceCreationEvent, null);
    }

    public static ObjectKey CreateVMTaskObjectKey(Server server, string wmiJobName, string instanceId)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return CreateObjectKey(server, server.VirtualizationNamespace, wmiJobName, "InstanceID", instanceId);
    }

    public static ObjectKey CreateInstalledEthernetSwitchExtensionObjectKey(Server server, string extensionId)
    {
        if (extensionId == null)
        {
            throw new ArgumentNullException("extensionId");
        }
        return CreateObjectKey(server, server.VirtualizationNamespace, "Msvm_InstalledEthernetSwitchExtension", "Name", extensionId);
    }

    public static ObjectKey CreateVMComputerSystemSettingObjectKey(Server server, string instanceId)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return CreateObjectKey(server, server.VirtualizationNamespace, "Msvm_VirtualSystemSettingData", "InstanceID", instanceId);
    }

    public static ObjectKey CreateSynthetic3DServiceObjectKey(Server server)
    {
        return CreateServiceObjectKey(server, "Msvm_Synthetic3DService", "synth3d", "Msvm_ComputerSystem");
    }

    private static ObjectKey CreateServiceObjectKey(Server server, string serviceClassName, string serviceName, string systemCreationClassName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        WmiObjectPath path = CreateServiceObjectPath(server, server.VirtualizationNamespace, serviceClassName, serviceName, systemCreationClassName);
        return new ObjectKey(server, path);
    }

    internal static WmiObjectPath CreateServiceObjectPath(Server server, string virtualizationNamespaceName, string serviceClassName, string serviceName, string systemCreationClassName)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        dictionary.Add("CreationClassName", serviceClassName);
        dictionary.Add("Name", serviceName);
        dictionary.Add("SystemCreationClassName", systemCreationClassName);
        dictionary.Add("SystemName", server.Name);
        return new WmiObjectPath(server, virtualizationNamespaceName, serviceClassName, dictionary);
    }

    private static ObjectKey CreateComputerSystemObjectKey(Server server, string wmiNamespace, string instanceId, string className)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (instanceId == null)
        {
            throw new ArgumentNullException("instanceId");
        }
        Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        dictionary.Add("CreationClassName", className);
        dictionary.Add("Name", instanceId);
        WmiObjectPath path = new WmiObjectPath(server, wmiNamespace, className, dictionary);
        return new ObjectKey(server, path);
    }

    private static ObjectKey CreateObjectKey(Server server, string wmiNamespace, string className, string keyProperty, string keyValue)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { keyProperty, keyValue } };
        WmiObjectPath path = new WmiObjectPath(server, wmiNamespace, className, keyValues);
        return new ObjectKey(server, path);
    }

    public static ObjectKey CreateClusterResourceObjectKey(Server server, string resourceName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (resourceName == null)
        {
            throw new ArgumentNullException("resourceName");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Name", resourceName } };
        WmiObjectPath path = new WmiObjectPath(server, Server.MSClusterNamespace, "MSCluster_Resource", keyValues);
        return new ObjectKey(server, path);
    }

    public static ObjectKey CreateClusterObjectKey(Server server, string clusterName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (clusterName == null)
        {
            throw new ArgumentNullException("clusterName");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Name", clusterName } };
        WmiObjectPath path = new WmiObjectPath(server, Server.MSClusterNamespace, "MSCluster_Cluster", keyValues);
        return new ObjectKey(server, path);
    }

    public static ObjectKey CreateClusterResourceGroupKey(Server server, string groupName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (groupName == null)
        {
            throw new ArgumentNullException("groupName");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Name", groupName } };
        WmiObjectPath path = new WmiObjectPath(server, Server.MSClusterNamespace, "MSCluster_ResourceGroup", keyValues);
        return new ObjectKey(server, path);
    }

    public static ObjectKey CreateClusterNodeObjectKey(Server server, string nodeName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (nodeName == null)
        {
            throw new ArgumentNullException("nodeName");
        }
        Dictionary<string, object> keyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Name", nodeName } };
        WmiObjectPath path = new WmiObjectPath(server, Server.MSClusterNamespace, "MSCluster_Node", keyValues);
        return new ObjectKey(server, path);
    }
}
