namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMObjectOrNameCmdlet : IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet
{
}
