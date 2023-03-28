using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_StorageJob")]
internal interface IVMStorageTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
	string Parent { get; }

	string Child { get; }
}
