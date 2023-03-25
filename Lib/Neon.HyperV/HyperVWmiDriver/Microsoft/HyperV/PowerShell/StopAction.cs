using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
internal enum StopAction
{
	TurnOff = 2,
	Save,
	ShutDown
}
