namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmBySingularVMNameCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	string VMName { get; set; }
}
