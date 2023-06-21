namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmByObjectCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    VirtualMachine[] VM { get; set; }
}
