using System;
using System.Collections.Generic;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IMSClusterClusterContract : IMSClusterCluster, IVirtualizationManagementObject
{
    public string Name => null;

    public string SharedVolumesRoot => null;

    public bool EnableSharedVolumes => false;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IEnumerable<IMSClusterNode> GetClusterNodes()
    {
        return null;
    }

    public IMSClusterReplicaBrokerResource GetReplicaBroker()
    {
        return null;
    }

    public IMSClusterVMResource GetVirtualMachineResource(string virtualMachineInstanceId)
    {
        return null;
    }

    public ClusterVerifyPathResult VerifyPath(string path, string groupName)
    {
        return ClusterVerifyPathResult.Valid;
    }

    public abstract void InvalidatePropertyCache();

    public abstract void UpdatePropertyCache();

    public abstract void UpdatePropertyCache(TimeSpan threshold);

    public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

    public abstract void UnregisterForInstanceModificationEvents();

    public abstract void InvalidateAssociationCache();

    public abstract void UpdateAssociationCache();

    public abstract void UpdateAssociationCache(TimeSpan threshold);

    public abstract string GetEmbeddedInstance();

    public abstract void DiscardPendingPropertyChanges();
}
