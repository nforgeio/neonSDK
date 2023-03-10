using System;

namespace Microsoft.HyperV.PowerShell;

[AttributeUsage(AttributeTargets.Property)]
internal class VirtualizationObjectIdentifierAttribute : Attribute
{
	public IdentifierFlags Flags { get; private set; }

	public VirtualizationObjectIdentifierAttribute(IdentifierFlags flags)
	{
		Flags = flags;
	}
}
