namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMMultipleSnapshotCmdlet
{
	VMSnapshot[] VMSnapshot { get; set; }
}
