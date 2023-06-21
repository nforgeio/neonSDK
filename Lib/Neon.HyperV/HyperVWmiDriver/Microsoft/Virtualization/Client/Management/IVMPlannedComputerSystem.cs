using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PlannedComputerSystem")]
internal interface IVMPlannedComputerSystem : IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
    IVMTask BeginRemoveFromGroupById(Guid collectionId);

    void EndRemoveFromGroupById(IVMTask task);
}
