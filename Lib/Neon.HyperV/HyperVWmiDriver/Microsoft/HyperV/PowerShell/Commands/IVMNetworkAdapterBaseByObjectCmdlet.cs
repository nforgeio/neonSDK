namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMNetworkAdapterBaseByObjectCmdlet : IVMNetworkAdapterBaseCmdlet, IParameterSet
{
	VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }
}
