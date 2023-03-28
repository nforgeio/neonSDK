using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MigrationJob")]
internal interface IVMMigrationTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
	string DestinationHost { get; }

	string VmComputerSystemInstanceId { get; }
}
