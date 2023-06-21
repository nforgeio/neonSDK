using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterCommonView : View
{
    internal static class ResourceTypeNames
    {
        public const string VirtualMachine = "Virtual Machine";

        public const string VirtualMachineConfiguration = "Virtual Machine Configuration";

        public const string WmiProvider = "Virtual Machine Cluster WMI";

        public const string BrokerResource = "Virtual Machine Replication Broker";

        public const string NetworkNameResource = "Network Name";
    }

    public string Name => GetProperty<string>("Name");

    public IEnumerable<IMSClusterNode> GetClusterNodes()
    {
        return GetRelatedObjects<IMSClusterNode>(base.Associations.ClusterToNode);
    }

    public IMSClusterVMResource GetVirtualMachineResource(string virtualMachineInstanceId)
    {
        string format = "SELECT * FROM {0} WHERE {1}.{2}='{3}' AND Type='{4}'";
        format = string.Format(CultureInfo.InvariantCulture, format, "MSCluster_Resource", "PrivateProperties", "VmID", virtualMachineInstanceId, "Virtual Machine");
        QueryAssociation association = QueryAssociation.CreateFromQuery(Server.MSClusterNamespace, format);
        return GetRelatedObject<IMSClusterVMResource>(association);
    }

    public IMSClusterReplicaBrokerResource GetReplicaBroker()
    {
        string format = "SELECT * FROM {0} WHERE Type='{1}'";
        string query = string.Format(CultureInfo.InvariantCulture, format, "MSCluster_Resource", "Virtual Machine Replication Broker");
        QueryAssociation association = QueryAssociation.CreateFromQuery(Server.MSClusterNamespace, query);
        return GetRelatedObject<IMSClusterReplicaBrokerResource>(association);
    }
}
