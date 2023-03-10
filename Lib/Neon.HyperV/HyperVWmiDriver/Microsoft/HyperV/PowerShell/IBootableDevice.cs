namespace Microsoft.HyperV.PowerShell;

internal interface IBootableDevice
{
	VMBootSource BootSource { get; }
}
