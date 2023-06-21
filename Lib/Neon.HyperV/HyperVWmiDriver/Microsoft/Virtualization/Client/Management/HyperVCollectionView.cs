#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class HyperVCollectionView : View, IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    private static class WmiMemberNames
    {
        internal const string InstanceId = "CollectionID";

        internal const string Name = "ElementName";
    }

    private readonly CollectionType m_Type;

    public Guid InstanceId => GetProperty("CollectionID", WmiTypeConverters.GuidStringConverter);

    public string Name
    {
        get
        {
            return GetProperty<string>("ElementName");
        }
        set
        {
            SetProperty("ElementName", value);
        }
    }

    public CollectionType Type => m_Type;

    protected HyperVCollectionView(CollectionType type)
    {
        m_Type = type;
    }

    public void Delete()
    {
        using IVMTask iVMTask = BeginDelete();
        iVMTask.WaitForCompletion();
        EndDelete(iVMTask);
    }

    public IVMTask BeginDelete()
    {
        IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.CurrentCulture, "Start deleting collection '{0}' ({1})", InstanceId.ToString(), Name));
        uint result = collectionManagementServiceProxy.InvokeMethod("DestroyCollection", array);
        return BeginMethodTaskReturn(result, null, array[1]);
    }

    public void EndDelete(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.DeleteCollection);
        VMTrace.TraceUserActionInitiated("Completed deleting collection");
        base.ProxyFactory.Repository.UnregisterProxy(base.Proxy);
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        IVMTask iVMTask = null;
        try
        {
            if (!properties.ContainsKey("ElementName"))
            {
                iVMTask = new CompletedTask(base.Server);
            }
            else
            {
                VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Begin rename of collection '{0}'", InstanceId.ToString("D")));
                string text = (string)properties["ElementName"];
                IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
                object[] array = new object[3] { this, text, null };
                uint result = collectionManagementServiceProxy.InvokeMethod("RenameCollection", array);
                iVMTask = BeginMethodTaskReturn(result, null, array[2]);
            }
        }
        catch (Exception wrappedException)
        {
            iVMTask = new CompletedTask(base.Server, wrappedException);
        }
        iVMTask.PutProperties = properties;
        return iVMTask;
    }
}
