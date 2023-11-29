namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVirtualMachineCmdlet : IServerParameters, IParameterSet
{
    VirtualMachineParameterType VirtualMachineParameterType { get; }
}
