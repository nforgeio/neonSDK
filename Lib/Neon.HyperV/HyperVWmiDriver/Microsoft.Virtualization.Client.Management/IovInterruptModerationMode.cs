namespace Microsoft.Virtualization.Client.Management;

internal enum IovInterruptModerationMode : uint
{
	Unknown = 0u,
	Adaptive = 1u,
	Off = 2u,
	Low = 100u,
	Medium = 200u,
	High = 300u
}
