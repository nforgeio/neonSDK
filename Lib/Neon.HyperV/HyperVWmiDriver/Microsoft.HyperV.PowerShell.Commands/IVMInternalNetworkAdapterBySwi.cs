using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMInternalNetworkAdapterBySwitchNameCmdlet : IVMNetworkAdapterBaseCmdlet, IParameterSet, IServerParameters
{
	SwitchParameter ManagementOS { get; set; }

	string SwitchName { get; set; }
}
