namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmByVMNameCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    string[] VMName { get; set; }
}
