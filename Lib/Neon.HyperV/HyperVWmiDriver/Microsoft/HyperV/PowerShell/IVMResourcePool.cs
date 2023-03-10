using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal interface IVMResourcePool
{
	string Name { get; }

	VMResourcePoolType ResourcePoolType { get; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly.")]
	string[] ParentName { get; }
}
