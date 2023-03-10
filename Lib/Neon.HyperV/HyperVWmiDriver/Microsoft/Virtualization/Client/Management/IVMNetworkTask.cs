using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ConcreteJob", PrimaryMapping = false)]
internal interface IVMNetworkTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
}
