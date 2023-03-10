using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemReferencePointExportJob")]
internal interface IVMExportReferencePointTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
}
