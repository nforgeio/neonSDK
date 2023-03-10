namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmBySingularNameCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	string Name { get; set; }
}
