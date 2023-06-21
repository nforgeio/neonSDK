using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemCollection")]
internal interface IVMCollection : IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    IEnumerable<IVMComputerSystem> CollectedVirtualMachines { get; }

    IVMTask BeginAddVirtualMachine(IVMComputerSystemBase virtualMachine);

    void EndAddVirtualMachine(IVMTask task);
}
