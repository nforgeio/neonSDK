using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMInternalNetworkAdapterCmdlet : IVMNetworkAdapterBaseCmdlet, IParameterSet, IServerParameters
{
	SwitchParameter ManagementOS { get; set; }
}
