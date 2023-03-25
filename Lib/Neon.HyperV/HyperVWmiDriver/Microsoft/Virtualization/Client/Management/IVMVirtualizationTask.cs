using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ConcreteJob")]
internal interface IVMVirtualizationTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
}
