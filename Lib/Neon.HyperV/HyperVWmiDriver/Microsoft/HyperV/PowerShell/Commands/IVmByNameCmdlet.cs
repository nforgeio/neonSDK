namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmByNameCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	string[] Name { get; set; }
}
