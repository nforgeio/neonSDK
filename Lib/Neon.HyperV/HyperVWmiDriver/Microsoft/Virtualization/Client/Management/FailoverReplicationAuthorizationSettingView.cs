#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class FailoverReplicationAuthorizationSettingView : View, IFailoverReplicationAuthorizationSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class WmiMemberNames
    {
        public const string AllowedPrimaryHostSystem = "AllowedPrimaryHostSystem";

        public const string TrustGroup = "TrustGroup";

        public const string ReplicaStorageLocation = "ReplicaStorageLocation";

        public const string ModifyAuthorizationEntry = "ModifyAuthorizationEntry";

        public const string RemoveAuthorizationEntry = "RemoveAuthorizationEntry";
    }

    public string AllowedPrimaryHostSystem
    {
        get
        {
            return GetProperty<string>("AllowedPrimaryHostSystem");
        }
        set
        {
            SetProperty("AllowedPrimaryHostSystem", value);
        }
    }

    public string TrustGroup
    {
        get
        {
            return GetProperty<string>("TrustGroup");
        }
        set
        {
            SetProperty("TrustGroup", value);
        }
    }

    public string ReplicaStorageLocation
    {
        get
        {
            return GetProperty<string>("ReplicaStorageLocation");
        }
        set
        {
            SetProperty("ReplicaStorageLocation", value);
        }
    }

    public IReplicationService Service => GetRelatedObject<IReplicationService>(base.Associations.ElementSettingData);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy failoverReplicationServiceProxy = GetFailoverReplicationServiceProxy();
        object[] array = new object[2] { embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyReplicationAuthSettingsFailed, AllowedPrimaryHostSystem);
        uint result = failoverReplicationServiceProxy.InvokeMethod("ModifyAuthorizationEntry", array);
        VMTrace.TraceUserActionInitiatedWithProperties("Modifying failover replication authorization settings.", properties);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public IVMTask BeginDelete()
    {
        IProxy failoverReplicationServiceProxy = GetFailoverReplicationServiceProxy();
        object[] array = new object[2] { AllowedPrimaryHostSystem, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteReplicationAuthSettingsFailed, AllowedPrimaryHostSystem);
        uint result = failoverReplicationServiceProxy.InvokeMethod("RemoveAuthorizationEntry", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndDelete(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.Delete);
        VMTrace.TraceUserActionCompleted("Deleting authorization entry completed successfully.");
    }

    public void Delete()
    {
        using IVMTask iVMTask = BeginDelete();
        iVMTask.WaitForCompletion();
        EndDelete(iVMTask);
    }
}
