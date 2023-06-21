#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMPlannedComputerSystemView : VMComputerSystemBaseView, IVMPlannedComputerSystem, IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
    public IVMTask BeginRemoveFromGroupById(Guid collectionId)
    {
        string text = collectionId.ToString("D");
        IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
        object[] array = new object[3] { this, text, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.CurrentCulture, "Start removing planned virtual machine from group {0}", text), base.ManagementPath.ToString());
        uint result = collectionManagementServiceProxy.InvokeMethod("RemoveMemberById", array);
        return BeginMethodTaskReturn(result, null, array[2]);
    }

    public void EndRemoveFromGroupById(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.RemoveMemberFromCollectionById);
        VMTrace.TraceUserActionCompleted("Completed removing a planned virtual machine from group");
    }
}
