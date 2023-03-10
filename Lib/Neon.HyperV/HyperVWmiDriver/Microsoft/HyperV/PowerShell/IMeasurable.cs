namespace Microsoft.HyperV.PowerShell;

internal interface IMeasurable : IMeasurableInternal
{
	bool ResourceMeteringEnabled { get; }
}
