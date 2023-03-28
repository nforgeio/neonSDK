using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_CopyFileToGuestJob")]
internal interface IVMCopyFileToGuestTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
}
