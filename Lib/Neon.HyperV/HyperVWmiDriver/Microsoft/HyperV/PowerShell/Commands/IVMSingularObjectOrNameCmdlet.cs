namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMSingularObjectOrNameCmdlet : IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularNameCmdlet
{
}
