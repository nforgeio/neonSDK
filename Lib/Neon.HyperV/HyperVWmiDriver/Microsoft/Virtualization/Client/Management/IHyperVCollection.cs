using System;

namespace Microsoft.Virtualization.Client.Management;

internal interface IHyperVCollection : IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    Guid InstanceId { get; }

    string Name { get; set; }

    CollectionType Type { get; }
}
