namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmBySingularObjectCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    VirtualMachine VM { get; set; }
}
