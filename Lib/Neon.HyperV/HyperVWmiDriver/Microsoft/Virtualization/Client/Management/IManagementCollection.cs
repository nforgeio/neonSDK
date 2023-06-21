using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ManagementCollection")]
internal interface IManagementCollection : IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    IEnumerable<IHyperVCollection> CollectedCollections { get; }

    IVMTask BeginAddCollection(IHyperVCollection collection);

    void EndAddCollection(IVMTask task);
}
