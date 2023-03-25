namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMObjectOrVMNameCmdlet : IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet
{
}
