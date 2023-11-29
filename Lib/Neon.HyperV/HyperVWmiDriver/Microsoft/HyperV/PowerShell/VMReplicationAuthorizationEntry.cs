using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMReplicationAuthorizationEntry : VirtualizationObject, IUpdatable, IRemovable
{
    private readonly DataUpdater<IFailoverReplicationAuthorizationSetting> m_Authorization;

    private string m_ClusterAllowedPrimaryServer;

    private string m_ClusterDefaultStorageLocation;

    private string m_ClusterTrustGroup;

    public string AllowedPrimaryServer
    {
        get
        {
            if (IsClusterEntry)
            {
                return m_ClusterAllowedPrimaryServer;
            }
            return m_Authorization.GetData(UpdatePolicy.EnsureUpdated).AllowedPrimaryHostSystem;
        }
    }

    public string ReplicaStorageLocation
    {
        get
        {
            if (IsClusterEntry)
            {
                return m_ClusterDefaultStorageLocation;
            }
            return m_Authorization.GetData(UpdatePolicy.EnsureUpdated).ReplicaStorageLocation;
        }
        internal set
        {
            if (IsClusterEntry)
            {
                m_ClusterDefaultStorageLocation = value;
            }
            else
            {
                m_Authorization.GetData(UpdatePolicy.None).ReplicaStorageLocation = value;
            }
        }
    }

    public string TrustGroup
    {
        get
        {
            if (IsClusterEntry)
            {
                return m_ClusterTrustGroup;
            }
            return m_Authorization.GetData(UpdatePolicy.EnsureUpdated).TrustGroup;
        }
        internal set
        {
            if (IsClusterEntry)
            {
                m_ClusterTrustGroup = value;
            }
            else
            {
                m_Authorization.GetData(UpdatePolicy.None).TrustGroup = value;
            }
        }
    }

    internal bool IsClusterEntry { get; private set; }

    internal VMReplicationAuthorizationEntry(IFailoverReplicationAuthorizationSetting authorization)
        : base(authorization)
    {
        m_Authorization = InitializePrimaryDataUpdater(authorization);
    }

    internal VMReplicationAuthorizationEntry(Server server, string allowedPrimaryServer, string replicaStorageLocation, string trustGroup)
        : base(server)
    {
        IsClusterEntry = true;
        m_ClusterAllowedPrimaryServer = allowedPrimaryServer;
        m_ClusterDefaultStorageLocation = replicaStorageLocation;
        m_ClusterTrustGroup = trustGroup;
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        if (IsClusterEntry)
        {
            VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(base.Server);
            VMReplicationAuthorizationEntry[] authorizationEntries = replicationServer.AuthorizationEntries;
            VMReplicationAuthorizationEntry[] array = authorizationEntries;
            foreach (VMReplicationAuthorizationEntry vMReplicationAuthorizationEntry in array)
            {
                if (string.Equals(AllowedPrimaryServer, vMReplicationAuthorizationEntry.AllowedPrimaryServer, StringComparison.OrdinalIgnoreCase))
                {
                    vMReplicationAuthorizationEntry.ReplicaStorageLocation = ReplicaStorageLocation;
                    vMReplicationAuthorizationEntry.TrustGroup = TrustGroup;
                }
            }
            replicationServer.CommitBrokerAuthorizationEntries(authorizationEntries, operationWatcher);
        }
        else
        {
            operationWatcher.PerformPut(m_Authorization.GetData(UpdatePolicy.None), TaskDescriptions.Task_SetVMAuthorizationEntry, null);
        }
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        if (IsClusterEntry)
        {
            VMReplicationServer.GetReplicationServer(base.Server).RemoveAuthorizationEntry(AllowedPrimaryServer, operationWatcher);
            return;
        }
        IFailoverReplicationAuthorizationSetting data = m_Authorization.GetData(UpdatePolicy.None);
        IReplicationService service = data.Service;
        operationWatcher.PerformDelete(data, TaskDescriptions.Task_DeleteVMAuthorizationEntry, null);
        service.InvalidateAssociationCache();
    }
}
