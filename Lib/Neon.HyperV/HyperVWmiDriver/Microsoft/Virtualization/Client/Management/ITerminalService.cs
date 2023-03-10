using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_TerminalService")]
internal interface ITerminalService : IVirtualizationManagementObject
{
	IVMTask BeginGrantVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees);

	IVMTask BeginRevokeVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees);

	void EndGrantVMConnectAccess(IVMTask task);

	void EndRevokeVMConnectAccess(IVMTask task);

	IEnumerable<IInteractiveSessionAccess> GetVMConnectAccess(IVMComputerSystem virtualMachine);
}
