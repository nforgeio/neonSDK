using System;

namespace Microsoft.Virtualization.Client.Management;

[Flags]
internal enum DisableOnFailoverFeature
{
    None = 0,
    RDMA = 1
}
