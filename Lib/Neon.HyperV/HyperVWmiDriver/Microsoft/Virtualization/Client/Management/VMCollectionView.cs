#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMCollectionView : HyperVCollectionView, IVMCollection, IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    public IEnumerable<IVMComputerSystem> CollectedVirtualMachines => GetRelatedObjects<IVMComputerSystem>(base.Associations.CollectedVirtualMachines);

    public VMCollectionView()
        : base(CollectionType.VMCollectionType)
    {
    }

    public IVMTask BeginAddVirtualMachine(IVMComputerSystemBase virtualMachine)
    {
        IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
        object[] array = new object[3] { virtualMachine, this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.CurrentCulture, "Start adding a virtual machine to '{0}' ({1})", base.InstanceId.ToString("D"), base.Name), virtualMachine.ManagementPath.ToString());
        uint result = collectionManagementServiceProxy.InvokeMethod("AddMember", array);
        return BeginMethodTaskReturn(result, null, array[2]);
    }

    public void EndAddVirtualMachine(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.AddMemberToCollection);
        VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.CurrentCulture, "Complete adding a virtual machine to '{0}' ({1})", base.InstanceId.ToString(), base.Name));
    }
}
