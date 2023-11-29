using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMExternalSwitchPortCmdlet : IVMNetworkAdapterBaseCmdlet, IParameterSet, IServerParameters
{
    SwitchParameter ExternalPort { get; set; }

    string SwitchName { get; set; }
}
